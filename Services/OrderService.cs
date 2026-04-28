using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.DTOs.Admin;
using ClientEcommerce.API.Enum;
using ClientEcommerce.API.Models;
using ClientEcommerce.API.Helpers;
using System.Text;


namespace ClientEcommerce.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IWhatsappService _whatsappService;

        public OrderService(AppDbContext context, IWhatsappService whatsappService)
        {
            _context = context;
            _whatsappService = whatsappService;
        }

        // ================= CUSTOMER =================

        public async Task PlaceOrder(int userId, PlaceOrderByCustomerDto dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var variants = _context.ProductVariants
                    .Where(v => dto.Items.Select(i => i.ProductVariantId).Contains(v.Id))
                    .ToDictionary(v => v.Id);

                var order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Placed.ToString(),
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                        throw new Exception("Invalid product variant");

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductVariantId = variant.Id,
                        Quantity = item.Quantity,
                        UnitPrice = variant.Price
                    });

                    totalAmount += variant.Price * item.Quantity;
                }

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                _context.SaveChanges();
                transaction.Commit();

                try
                {
                    var detailedOrder = await _context.Orders
                        .AsNoTracking()
                        .Include(o => o.User)
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.ProductVariant)
                                .ThenInclude(pv => pv.Product)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);

                    var userPhone = detailedOrder?.User?.PhoneNumber;
                    var userName = detailedOrder?.User?.Name;
                    var userEmail = detailedOrder?.User?.Email;
                    var userCompany = detailedOrder?.User?.CompanyName;

                    var adminNumbers = await _whatsappService.GetAdminWhatsappNumbers();
                    if (adminNumbers.Count > 0)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("New order placed");
                        sb.AppendLine($"OrderId: {order.Id}");
                        sb.AppendLine($"Total: {order.TotalAmount}");
                        sb.AppendLine($"Items: {order.OrderItems.Count}");
                        sb.AppendLine("--- Customer ---");
                        if (!string.IsNullOrWhiteSpace(userName)) sb.AppendLine($"Name: {userName}");
                        if (!string.IsNullOrWhiteSpace(userCompany)) sb.AppendLine($"Company: {userCompany}");
                        if (!string.IsNullOrWhiteSpace(userPhone)) sb.AppendLine($"Phone: {userPhone}");
                        if (!string.IsNullOrWhiteSpace(userEmail)) sb.AppendLine($"Email: {userEmail}");
                        sb.AppendLine("--- Items ---");

                        if (detailedOrder?.OrderItems != null && detailedOrder.OrderItems.Count > 0)
                        {
                            foreach (var oi in detailedOrder.OrderItems)  
                            {
                                var productName = oi.ProductVariant?.Product?.Name ?? "";
                                var variantName = oi.ProductVariant?.Size ?? "";
                                sb.AppendLine($"- {productName} {variantName} | Qty: {oi.Quantity} | Price: {oi.UnitPrice}");
                            }
                        }

                        var msg = sb.ToString().Trim();
                        foreach (var adminTo in adminNumbers)
                        {
                            await _whatsappService.SendWhatsapp(adminTo, msg);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(userPhone))
                    {
                        var customerMsg = $"Your order has been placed successfully. OrderId: {order.Id}, Total: {order.TotalAmount}";
                        await _whatsappService.SendWhatsapp(userPhone, customerMsg);
                    }
                    else
                    {
                        System.Console.WriteLine($"[WhatsApp customer skipped] UserId={userId} has no phone number.");
                    }

                    var debugTo = Environment.GetEnvironmentVariable("WHATSAPP_DEBUG_TO");
                    if (!string.IsNullOrWhiteSpace(debugTo))
                    {
                        await _whatsappService.SendWhatsapp(debugTo, "Test message after order placement");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[WhatsApp error] Failed to notify admin for OrderId={order.Id}. {ex}");
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<UserOrderListDto> GetOrdersForUser(int userId)
        {
            return _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new UserOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToList();
        }

        public UserOrderDetailDto GetOrderForUser(int orderId, int userId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId)
                ?? throw new Exception("Order not found");

            return new UserOrderDetailDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            };
        }

        public async Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Images)
                .Select(o => new OrderDetailsDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    DeliveredDate = o.DeliveredAt,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductVariant.Product.Id,
                        ProductName = i.ProductVariant.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.UnitPrice * i.Quantity,
                        ProductImage = i.ProductVariant.Product.Images
                            .OrderByDescending(img => img.IsPrimary)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault()
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // ================= ADMIN =================

        public void ConfirmOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != OrderStatus.Placed.ToString())
                throw new Exception("Only placed orders can be confirmed");

            order.Status = OrderStatus.Confirmed.ToString();
            _context.SaveChanges();
        }

        public void CancelOrder(int orderId, string reason)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            order.Status = OrderStatus.Cancelled.ToString();
            _context.SaveChanges();
        }

        public void DispatchOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != OrderStatus.Confirmed.ToString())
                throw new Exception("Order must be confirmed before dispatch");

            order.Status = OrderStatus.Dispatched.ToString();
            order.DispatchedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void DeliverOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != OrderStatus.Dispatched.ToString())
                throw new Exception("Order must be dispatched before delivery");

            order.Status = OrderStatus.Delivered.ToString();
            order.DeliveredAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public AdminOrderDetailDto GetOrderById(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.Id == orderId)
                ?? throw new Exception("Order not found");

            return new AdminOrderDetailDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CustomerName = order.User.Name,
                CompanyName = order.User.CompanyName,
                PhoneNumber = order.User.PhoneNumber,
                Items = order.OrderItems.Select(i => new AdminOrderItemDto
                {
                    ProductName = i.ProductVariant.Product.Name,
                    Size = i.ProductVariant.Size,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }


        public async Task<PagedResultDto<AdminOrderListDto>> GetAdminOrders(int page, int pageSize, string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Pending")
                    query = query.Where(o => o.Status == OrderStatus.Placed.ToString());
                else
                    query = query.Where(o => o.Status == status);
            }


            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new AdminOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = OrderStatusHelper.GetCustomerStatus(o.Status),

                    TotalAmount = o.TotalAmount,
                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,
                    PhoneNumber = o.User.PhoneNumber
                })
                .ToListAsync();

            return new PagedResultDto<AdminOrderListDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }


        public IEnumerable<AdminOrderListDto> GetRecentOrders(int count)
        {
            return _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new AdminOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,
                    PhoneNumber = o.User.PhoneNumber
                })
                .ToList();
        }
    }
}
