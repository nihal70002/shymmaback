using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace ClientEcommerce.API.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)

                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // 1️⃣ Try local connection string first
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            // 2️⃣ Fallback to Railway DATABASE_URL
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = configuration["DATABASE_URL"];
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string not found. " +
                    "Set ConnectionStrings:DefaultConnection or DATABASE_URL.");
            }

            // 3️⃣ Convert Railway postgres:// URL if needed
            if (connectionString.StartsWith("postgres://"))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':', 2);

                connectionString =
                    $"Host={uri.Host};" +
                    $"Port={uri.Port};" +
                    $"Database={uri.AbsolutePath.Trim('/')};" +
                    $"Username={userInfo[0]};" +
                    $"Password={userInfo[1]};" +
                    $"SslMode=Require;" +
                    $"Trust Server Certificate=true";
            }

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
