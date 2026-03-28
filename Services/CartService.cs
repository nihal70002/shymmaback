using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;
namespace ClientEcommerce.API.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        public CartService(AppDbContext context)
        {
            _context = context;
        }
        // ================= ADD TO CART =================
        public void AddToCart(int userId, AddToCartDto dto)
        {
            Console.WriteLine("===== ADD TO CART DEBUG START =====");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"DTO ProductVariantId: {dto.ProductVariantId}");
            Console.WriteLine($"DTO Quantity: {dto.Quantity}");

            // 1️⃣ Get or create cart
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
                Console.WriteLine($"New cart created. CartId: {cart.Id}");
            }
            else
            {
                Console.WriteLine($"Existing cart found. CartId: {cart.Id}");
            }

            // 2️⃣ Check ALL ProductVariants in DB
            var allVariants = _context.ProductVariants.ToList();
            Console.WriteLine($"Total variants in DB: {allVariants.Count}");

            foreach (var v in allVariants)
            {
                Console.WriteLine(
                    $"VariantId: {v.Id}, ProductId: {v.ProductId}, Size: {v.Size}"
                );
            }

            // 3️⃣ Try to find variant
            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.Id == dto.ProductVariantId);

            if (variant == null)
            {
                Console.WriteLine("❌ VARIANT NOT FOUND — STOPPING HERE");
                Console.WriteLine("===== ADD TO CART DEBUG END =====");
                return; // ⛔ TEMP: do NOT throw
            }

            Console.WriteLine($"✅ Variant found: VariantId {variant.Id}");

            // 4️⃣ Check existing cart item
            var existingItem = _context.CartItems.FirstOrDefault(i =>
                i.CartId == cart.Id &&
                i.ProductVariantId == variant.Id
            );

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                Console.WriteLine("Updated existing cart item");
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
                Console.WriteLine("New cart item added");
            }

            _context.SaveChanges();
            Console.WriteLine("===== ADD TO CART DEBUG END =====");
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
                    Quantity = i.Quantity,
                    Price = i.Price,

                    // ✅ Primary product image
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