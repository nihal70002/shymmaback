using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;

namespace ClientEcommerce.API.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(AppDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= ADD TO CART =================
        public void AddToCart(int userId, AddToCartDto dto)
        {
            // 1️⃣ Get or create cart
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
                _logger.LogInformation("New cart created for user {UserId}", userId);
            }

            // 2️⃣ Find the variant directly (no need to load all)
            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.Id == dto.ProductVariantId);

            if (variant == null)
            {
                _logger.LogWarning("Variant {VariantId} not found for add-to-cart", dto.ProductVariantId);
                throw new NotFoundException("Product variant not found");
            }

            // 3️⃣ Check existing cart item
            var existingItem = _context.CartItems.FirstOrDefault(i =>
                i.CartId == cart.Id &&
                i.ProductVariantId == variant.Id
            );

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                _logger.LogInformation("Updated cart item quantity for user {UserId}", userId);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variant.Id,
                    Quantity = dto.Quantity,
                    Price = variant.Price
                };
                _context.CartItems.Add(cartItem);
                _logger.LogInformation("New cart item added for user {UserId}", userId);
            }

            _context.SaveChanges();
        }

        // ================= GET CART =================
        public IEnumerable<CartItemDto> GetCart(int userId)
        {
            return _context.CartItems
                .AsNoTracking()
                .Include(i => i.Cart)
                .Include(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Images)
                .Where(i => i.Cart.UserId == userId)
                .Select(i => new CartItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name,
                    Size = i.ProductVariant.Size,
                    Material = i.ProductVariant.Material,
                    Class = i.ProductVariant.Class,
                    Color = i.ProductVariant.Color,
                    ProductCode = i.ProductVariant.ProductCode,
                    Quantity = i.Quantity,
                    ProductId = i.ProductVariant.ProductId,
                    Price = i.Price,

                    ImageUrl = i.ProductVariant.Product.Images
        .OrderByDescending(img => img.IsPrimary)
        .Select(img => img.ImageUrl)
        .FirstOrDefault()
                })
                .ToList();
        }

        // ================= REMOVE ITEM =================
        public void RemoveItem(int userId, int productVariantId)
        {
            var item = _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefault(i =>
            i.Cart.UserId == userId &&
            i.ProductVariantId == productVariantId);
            if (item == null) return;
            _context.CartItems.Remove(item);
            _context.SaveChanges();
        }
        // ================= UPDATE QUANTITY =================
        public void UpdateQuantity(int userId, UpdateCartQuantityDto dto)
        {
            var item = _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefault(i =>
            i.Cart.UserId == userId &&
            i.ProductVariantId == dto.ProductVariantId);
            if (item == null) return;
            if (dto.Quantity <= 0)
                _context.CartItems.Remove(item);
            else
                item.Quantity = dto.Quantity;
            _context.SaveChanges();
        }
        // ================= CLEAR CART =================
        public void ClearCart(int userId)
        {
            var items = _context.CartItems
            .Include(i => i.Cart)
            .Where(i => i.Cart.UserId == userId)
            .ToList();
            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                _context.SaveChanges();
            }
        }
    }
}