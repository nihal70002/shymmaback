using Microsoft.AspNetCore.Identity;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientEcommerce.API.Seed
{
    public static class DbSeeder
    {
        public static void SeedAdmin(AppDbContext context)
        {



            Console.WriteLine("Database: " + context.Database.GetDbConnection().Database);
            Console.WriteLine("DataSource: " + context.Database.GetDbConnection().DataSource);

            if (context.Users.Any(u => u.Email == "admin@company.com"))
            {
                Console.WriteLine("✅ Admin user already exists. Skipping seed.");
                return;
            }

            var hasher = new PasswordHasher<User>();

            var admin = new User
            {
                Name = "Super Admin",
                Email = "admin@company.com",
                CompanyName = "Private E-Commerce",
                PhoneNumber = "7591907000",
                Role = "Admin",
                PasswordHash = hasher.HashPassword(null!, "Admin@123"),
                IsActive = true
            };


            context.Users.Add(admin);
            context.SaveChanges();

            Console.WriteLine("🚀 Admin user seeded successfully (admin@company.com)");
        }
    }
}
