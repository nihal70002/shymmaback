public interface IAuthService
{
    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    Task ForgotPasswordAsync(string email);

    Task ResetPasswordAsync(string token, string newPassword);
}
