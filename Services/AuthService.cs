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
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IEmailService emailService,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
        _logger = logger;
        _passwordHasher = new PasswordHasher<User>();
    }

    // 🔐 CHANGE PASSWORD (already logged in)
    public async Task ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword)
    {
        if (!int.TryParse(userId, out var id))
            throw new BadRequestException("Invalid user id");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new NotFoundException("User not found");

        var verify = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            currentPassword
        );

        if (verify == PasswordVerificationResult.Failed)
            throw new BadRequestException("Current password is incorrect");

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _context.SaveChangesAsync();
    }

    // 📧 FORGOT PASSWORD
    public async Task ForgotPasswordAsync(string email)
    {
        email = email.Trim().ToLower(); // 🔑 normalize input

        _logger.LogInformation("ForgotPasswordAsync called for: {Email}", email);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
        {
            _logger.LogWarning("User not found for forgot password: {Email}", email);
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

        _logger.LogInformation("Sending reset email to {Email}", user.Email);

        await _emailService.SendAsync(
            user.Email,
            "Reset your PrivateCommerce password",
            $"Click here to reset your password: {resetLink}"
        );

        _logger.LogInformation("Reset email sent to {Email}", user.Email);
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
            throw new BadRequestException("Invalid or expired token");

        reset.User.PasswordHash =
            _passwordHasher.HashPassword(reset.User, newPassword);

        reset.IsUsed = true;
        await _context.SaveChangesAsync();
    }
}
