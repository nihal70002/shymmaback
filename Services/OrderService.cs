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
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, IWhatsappService whatsappService, ILogger<OrderService> logger)
        {
            _context = context;
            _whatsappService = whatsappService;
            _logger = logger;
        }

        // ================= CUSTOMER =================

        public async Task PlaceOrder(int userId, PlaceOrderByCustomerDto dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var variants = _context.ProductVariants
                    .Include(v => v.Product)
                    .Where(v => dto.Items.Select(i => i.ProductVariantId).Contains(v.Id))
                    .ToDictionary(v => v.Id);

                

                var order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Placed.ToString(),
                    OrderDate = DateTime.UtcNow,
                    PreferredDeliveryDate = dto.PreferredDeliveryDate.HasValue ? 
                        DateTime.SpecifyKind(dto.PreferredDeliveryDate.Value, DateTimeKind.Utc) : null,
                    PreferredDeliveryTime = dto.PreferredDeliveryTime,
                    DeliveryInstructions = dto.DeliveryInstructions,
                    SurgeonName = dto.SurgeonName,
                    HospitalName = dto.HospitalName,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                        throw new BadRequestException("Invalid product variant");

                    if (string.IsNullOrWhiteSpace(variant.Style) || string.IsNullOrWhiteSpace(variant.Material))
                        throw new BadRequestException("Please select style and material before placing this order");

                    var orderItem = new OrderItem
                    {
                        ProductVariantId = variant.Id,
                        Quantity = item.Quantity,
                        UnitPrice = variant.Price
                    };
                    SetVariantSnapshot(orderItem, variant);
                    order.OrderItems.Add(orderItem);

                    totalAmount += variant.Price * item.Quantity;
                }

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                _context.SaveChanges();
                transaction.Commit();

                try
                {
                    Console.WriteLine("Starting WhatsApp notifications for order...");
                    
                    var detailedOrder = await _context.Orders
                        .AsNoTracking()
                        .Include(o => o.User)
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.ProductVariant)
                                .ThenInclude(pv => pv.Product)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(o => o.Id == order.Id);

                    var userPhone = detailedOrder?.User?.PhoneNumber;
                    var userName = detailedOrder?.User?.Name;
                    var userEmail = detailedOrder?.User?.Email;
                    var userCompany = detailedOrder?.User?.CompanyName;

                    Console.WriteLine($"User phone: {userPhone}");
                    Console.WriteLine($"User name: {userName}");

                    var adminNumbers = await _whatsappService.GetAdminWhatsappNumbers();
                    Console.WriteLine($"Admin numbers count: {adminNumbers.Count}");
                    
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
                                var productName = oi.ProductNameSnapshot ?? oi.ProductVariant?.Product?.Name ?? "";
                                var variantName = BuildVariantDescription(
                                    oi.SizeSnapshot ?? oi.ProductVariant?.Size,
                                    oi.StyleSnapshot ?? oi.ProductVariant?.Style,
                                    oi.MaterialSnapshot ?? oi.ProductVariant?.Material,
                                    oi.ColorSnapshot ?? oi.ProductVariant?.Color,
                                    oi.ClassSnapshot ?? oi.ProductVariant?.Class);
                                sb.AppendLine($"- {productName} {variantName} | Qty: {oi.Quantity} | Price: {oi.UnitPrice}");
                            }
                        }

                        // Add delivery preferences to admin message
                        if (order.PreferredDeliveryDate.HasValue || 
                            !string.IsNullOrWhiteSpace(order.PreferredDeliveryTime) || 
                            !string.IsNullOrWhiteSpace(order.DeliveryInstructions) ||
                            !string.IsNullOrWhiteSpace(order.SurgeonName) ||
                            !string.IsNullOrWhiteSpace(order.HospitalName))
                        {
                            sb.AppendLine("--- Delivery Preferences ---");
                            if (order.PreferredDeliveryDate.HasValue)
                                sb.AppendLine($"Date: {order.PreferredDeliveryDate.Value:yyyy-MM-dd}");
                            if (!string.IsNullOrWhiteSpace(order.PreferredDeliveryTime))
                                sb.AppendLine($"Time: {order.PreferredDeliveryTime}");
                            if (!string.IsNullOrWhiteSpace(order.DeliveryInstructions))
                                sb.AppendLine($"Instructions: {order.DeliveryInstructions}");
                            if (!string.IsNullOrWhiteSpace(order.SurgeonName))
                                sb.AppendLine($"Surgeon: {order.SurgeonName}");
                            if (!string.IsNullOrWhiteSpace(order.HospitalName))
                                sb.AppendLine($"Hospital: {order.HospitalName}");
                        }

                        var msg = sb.ToString().Trim();
                        Console.WriteLine($"Admin message: {msg}");
                        
                        foreach (var adminTo in adminNumbers)
                        {
                            Console.WriteLine($"Sending WhatsApp to admin: {adminTo}");
                            await _whatsappService.SendWhatsapp(adminTo, msg);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(userPhone))
                    {
                        var customerMsg = $"Your order has been placed successfully. OrderId: {order.Id}, Total: {order.TotalAmount}";

                        if (detailedOrder?.OrderItems != null && detailedOrder.OrderItems.Count > 0)
                        {
                            customerMsg += "\n\nItems:";
                            foreach (var oi in detailedOrder.OrderItems)
                            {
                                var productName = oi.ProductNameSnapshot ?? oi.ProductVariant?.Product?.Name ?? "";
                                var variantName = BuildVariantDescription(
                                    oi.SizeSnapshot ?? oi.ProductVariant?.Size,
                                    oi.StyleSnapshot ?? oi.ProductVariant?.Style,
                                    oi.MaterialSnapshot ?? oi.ProductVariant?.Material,
                                    oi.ColorSnapshot ?? oi.ProductVariant?.Color,
                                    oi.ClassSnapshot ?? oi.ProductVariant?.Class);
                                customerMsg += $"\n- {productName} {variantName} | Qty: {oi.Quantity}";
                            }
                        }
                        
                        // Add delivery preferences to customer message
                        if (order.PreferredDeliveryDate.HasValue || 
                            !string.IsNullOrWhiteSpace(order.PreferredDeliveryTime) || 
                            !string.IsNullOrWhiteSpace(order.DeliveryInstructions) ||
                            !string.IsNullOrWhiteSpace(order.SurgeonName) ||
                            !string.IsNullOrWhiteSpace(order.HospitalName))
                        {
                            customerMsg += "\n\n📦 Delivery Preferences:";
                            if (order.PreferredDeliveryDate.HasValue)
                                customerMsg += $"\n📅 Date: {order.PreferredDeliveryDate.Value:yyyy-MM-dd}";
                            if (!string.IsNullOrWhiteSpace(order.PreferredDeliveryTime))
                                customerMsg += $"\n⏰ Time: {order.PreferredDeliveryTime}";
                            if (!string.IsNullOrWhiteSpace(order.DeliveryInstructions))
                                customerMsg += $"\n📝 Instructions: {order.DeliveryInstructions}";
                            if (!string.IsNullOrWhiteSpace(order.SurgeonName))
                                customerMsg += $"\n👨‍⚕️ Surgeon: {order.SurgeonName}";
                            if (!string.IsNullOrWhiteSpace(order.HospitalName))
                                customerMsg += $"\n🏥 Hospital: {order.HospitalName}";
                        }
                        
                        Console.WriteLine($"Customer message: {customerMsg}");
                        Console.WriteLine($"Sending WhatsApp to customer: {userPhone}");
                        
                        await _whatsappService.SendWhatsapp(userPhone, customerMsg);
                    }
                    else
                    {
                        Console.WriteLine("WhatsApp customer skipped: User has no phone number");
                        _logger.LogInformation("WhatsApp customer skipped: UserId={UserId} has no phone number", userId);
                    }

                    var debugTo = Environment.GetEnvironmentVariable("WHATSAPP_DEBUG_TO");
                    if (!string.IsNullOrWhiteSpace(debugTo))
                    {
                        await _whatsappService.SendWhatsapp(debugTo, "Test message after order placement");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WhatsApp notification failed for OrderId={OrderId}", order.Id);
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
                .AsNoTracking()
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
                ?? throw new NotFoundException("Order not found");

            return new UserOrderDetailDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PreferredDeliveryDate = order.PreferredDeliveryDate,
                PreferredDeliveryTime = order.PreferredDeliveryTime,
                DeliveryInstructions = order.DeliveryInstructions,
                SurgeonName = order.SurgeonName,
                HospitalName = order.HospitalName
            };
        }

        public async Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Select(o => new OrderDetailsDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    DeliveredDate = o.DeliveredAt,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    PreferredDeliveryDate = o.PreferredDeliveryDate,
                    PreferredDeliveryTime = o.PreferredDeliveryTime,
                    DeliveryInstructions = o.DeliveryInstructions,
                    SurgeonName = o.SurgeonName,
                    HospitalName = o.HospitalName,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductVariant.Product.Id,
                        ProductName = i.ProductNameSnapshot ?? i.ProductVariant.Product.Name,
                        Size = i.SizeSnapshot ?? i.ProductVariant.Size,
                        Style = i.StyleSnapshot ?? i.ProductVariant.Style,
                        Material = i.MaterialSnapshot ?? i.ProductVariant.Material,
                        Color = i.ColorSnapshot ?? i.ProductVariant.Color,
                        Class = i.ClassSnapshot ?? i.ProductVariant.Class,
                        ProductCode = i.ProductCodeSnapshot ?? i.ProductVariant.ProductCode,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }


        public async Task<PagedResultDto<AdminOrderListDto>> GetAdminOrders(int page, int pageSize, string? status)
        {
            var query = _context.Orders
                .AsNoTracking()
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
                .AsNoTracking()
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

        public void CancelCustomerOrder(int userId, int orderId, string reason)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found");

            if (order.Status != OrderStatus.Placed.ToString())
                throw new BadRequestException("Only placed orders can be cancelled");

            order.Status = OrderStatus.Cancelled.ToString();
            _context.SaveChanges();
        }

        public void ConfirmOrder(int orderId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found");

            if (order.Status != OrderStatus.Placed.ToString())
                throw new BadRequestException("Only placed orders can be confirmed");

            order.Status = OrderStatus.Confirmed.ToString();
            _context.SaveChanges();
        }

        public void CancelOrder(int orderId, string reason)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found");

            if (order.Status != OrderStatus.Placed.ToString() && order.Status != OrderStatus.Confirmed.ToString())
                throw new BadRequestException("Only placed or confirmed orders can be cancelled");

            order.Status = OrderStatus.Cancelled.ToString();
            _context.SaveChanges();
        }

        public void DispatchOrder(int orderId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found");

            if (order.Status != OrderStatus.Confirmed.ToString())
                throw new BadRequestException("Only confirmed orders can be dispatched");

            order.Status = OrderStatus.Dispatched.ToString();
            _context.SaveChanges();
        }

        public void DeliverOrder(int orderId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found");

            if (order.Status != OrderStatus.Dispatched.ToString())
                throw new BadRequestException("Only dispatched orders can be delivered");

            order.Status = OrderStatus.Delivered.ToString();
            order.DeliveredAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public async Task<AdminOrderDetailDto?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Select(o => new AdminOrderDetailDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    PreferredDeliveryDate = o.PreferredDeliveryDate,
                    PreferredDeliveryTime = o.PreferredDeliveryTime,
                    DeliveryInstructions = o.DeliveryInstructions,
                    SurgeonName = o.SurgeonName,
                    HospitalName = o.HospitalName,
                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,
                    PhoneNumber = o.User.PhoneNumber,
                    Email = o.User.Email,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductVariant.Product.Id,
                        ProductName = i.ProductNameSnapshot ?? i.ProductVariant.Product.Name,
                        Size = i.SizeSnapshot ?? i.ProductVariant.Size,
                        Style = i.StyleSnapshot ?? i.ProductVariant.Style,
                        Material = i.MaterialSnapshot ?? i.ProductVariant.Material,
                        Color = i.ColorSnapshot ?? i.ProductVariant.Color,
                        Class = i.ClassSnapshot ?? i.ProductVariant.Class,
                        ProductCode = i.ProductCodeSnapshot ?? i.ProductVariant.ProductCode,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

    private decimal CalculateShippingCharge(decimal orderTotal)
    {
        // Free shipping for orders above ₹499
        if (orderTotal >= 499m)
        {
            return 0m;
        }
        
        // Shipping charge for orders below ₹499
        // You can customize the shipping charge logic here
        return 40m; // ₹40 shipping charge for orders below ₹499
    }

        private static void SetVariantSnapshot(OrderItem item, ProductVariant variant)
        {
            item.ProductNameSnapshot = variant.Product.Name;
            item.SizeSnapshot = variant.Size;
            item.StyleSnapshot = variant.Style;
            item.MaterialSnapshot = variant.Material;
            item.ColorSnapshot = variant.Color;
            item.ClassSnapshot = variant.Class;
            item.ProductCodeSnapshot = variant.ProductCode;
        }

        private static string BuildVariantDescription(
            string? size,
            string? style,
            string? material,
            string? color,
            string? classValue)
        {
            var parts = new[] { size, style, material, color, classValue }
                .Where(v => !string.IsNullOrWhiteSpace(v));

            return string.Join(" / ", parts);
        }
    }
}
