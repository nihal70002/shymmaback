using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ClientEcommerce.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        try
        {
            var host = _config["Smtp:Host"];
            var port = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                throw new Exception("SMTP configuration is missing");
            }

            using var smtp = new SmtpClient(host, int.Parse(port))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                Timeout = 10000 // 10 seconds
            };

            using var message = new MailMessage
            {
                From = new MailAddress(user, "PrivateCommerce"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await smtp.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // 🔍 log only, do NOT throw
            Console.WriteLine($"Email send failed: {ex.Message}");
            throw; // let AuthService decide what to do
        }
    }
}
