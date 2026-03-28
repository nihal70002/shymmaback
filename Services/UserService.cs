using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.DTOs.Admin;
using ClientEcommerce.API.Models;

namespace ClientEcommerce.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // ============================
        // ADMIN
        // ============================

        /// <summary>
        /// Create a normal end user (customer)
        /// </summary>
        public void CreateUser(CreateUserDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyName = dto.CompanyName,
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        /// <summary>
        /// Admin: Get all users
        /// </summary>
        public List<AdminUserDto> GetAllUsers()
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "User")
                .Select(u => new AdminUserDto
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CompanyName = u.CompanyName,
                    IsActive = u.IsActive
                })
                .ToList();
        }

        /// <summary>
        /// Admin: Get single user details with order summary
        /// </summary>
        public UserDetailsDto? GetUserDetails(int userId)
        {
            var user = _context.Users
                .Include(u => u.OrdersPlaced)
                .FirstOrDefault(u => u.Id == userId && u.Role == "User");

            if (user == null)
                return null;

            return new UserDetailsDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                OrderHistory = user.OrdersPlaced.Select(o => new OrderHistoryDto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToList()
            };
        }

        /// <summary>
        /// Admin: Enable / disable user
        /// </summary>
        public void ToggleUserStatus(int userId)
        {
            var user = _context.Users.Find(userId)
                ?? throw new Exception("User not found");

            user.IsActive = !user.IsActive;
            _context.SaveChanges();
        }

        // ============================
        // USER
        // ============================

        public UserProfileDto GetProfile(int userId)
        {
            var user = _context.Users.Find(userId)
                ?? throw new Exception("User not found");

            return new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public void UpdateProfile(int userId, UpdateUserProfileDto dto)
        {
            var user = _context.Users.Find(userId)
                ?? throw new Exception("User not found");

            user.Name = dto.Name;
            user.PhoneNumber = dto.PhoneNumber;

            _context.SaveChanges();
        }

        public void ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = _context.Users.Find(userId)
                ?? throw new Exception("User not found");

            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.NewPassword);

            _context.SaveChanges();
        }
    }
}
