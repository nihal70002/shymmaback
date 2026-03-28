using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.Models;
using ClientEcommerce.API.Services;


public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthService(
        AppDbContext context,
        IEmailService emailService,
        IConfiguration config)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
        _passwordHasher = new PasswordHasher<User>();
    }

    // 🔐 CHANGE PASSWORD (already logged in)
    public async Task ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword)
    {
        if (!int.TryParse(userId, out var id))
            throw new Exception("Invalid user id");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("User not found");

        var verify = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            currentPassword
        );

        if (verify == PasswordVerificationResult.Failed)
            throw new Exception("Current password is incorrect");

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _context.SaveChangesAsync();
    }

    // 📧 FORGOT PASSWORD
    public async Task ForgotPasswordAsync(string email)
    {
        email = email.Trim().ToLower(); // 🔑 normalize input

        Console.WriteLine($"ForgotPasswordAsync called with: {email}");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
        {
            Console.WriteLine("User NOT found for forgot password");
            return;
        }

        var token = Guid.NewGuid().ToString();

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            Expiry = DateTime.UtcNow.AddMinutes(30),
            IsUsed = false
        });

        await _context.SaveChangesAsync();

        var resetLink =
            $"{_config["FrontendUrl"]}/reset-password?token={token}";

        Console.WriteLine($"Sending reset email to {user.Email}");

        await _emailService.SendAsync(
            user.Email,
            "Reset your PrivateCommerce password",
            $"Click here to reset your password: {resetLink}"
        );

        Console.WriteLine("Reset email SENT");
    }


    // 🔄 RESET PASSWORD
    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var reset = await _context.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Token == token &&
                !x.IsUsed &&
                x.Expiry > DateTime.UtcNow);

        if (reset == null)
            throw new Exception("Invalid or expired token");

        reset.User.PasswordHash =
            _passwordHasher.HashPassword(reset.User, newPassword);

        reset.IsUsed = true;
        await _context.SaveChangesAsync();
    }
}
