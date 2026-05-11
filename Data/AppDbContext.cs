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

        public DbSet<AllowedWhatsappUser> AllowedWhatsappUsers { get; set; }

        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVideo> ProductVideos { get; set; }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<ProductComponent> ProductComponents { get; set; }
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

            modelBuilder.Entity<ProductVideo>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Videos)
                .HasForeignKey(pv => pv.ProductId)
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

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.ProductVariantId);

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductVariantId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductCode)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.BrandId);

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ProductId);

            modelBuilder.Entity<ProductVideo>()
                .HasIndex(pv => pv.ProductId);

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(v => v.ProductId);

            // ================= ADDITIONAL PERFORMANCE INDEXES =================

            modelBuilder.Entity<Address>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(p => p.Token);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(p => p.UserId);

            modelBuilder.Entity<ProductComponent>()
                .HasIndex(pc => pc.ProductId);
        }

    }

}
