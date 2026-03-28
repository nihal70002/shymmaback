using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Models;

namespace ClientEcommerce.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= CART =================
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= PRODUCT =================
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // SKU UNIQUE INDEX (Very Important)
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(v => v.ProductCode)
                .IsUnique();

            // ================= USER =================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // ================= CATEGORY =================
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔥 IMPORTANT: Add index for faster subcategory lookup
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.ParentCategoryId);

            // ================= ORDER =================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.OrdersPlaced)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);
        }

    }

}
