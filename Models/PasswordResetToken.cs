using ClientEcommerce.API.Models;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expiry { get; set; }
    public bool IsUsed { get; set; } = false;

    public User User { get; set; } = null!;
}
