using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ClientEcommerce.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
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
                throw new BadRequestException("SMTP configuration is missing");
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
            _logger.LogError(ex, "Email send failed to {Recipient}", to);
            throw; // let AuthService decide what to do
        }
    }
}
