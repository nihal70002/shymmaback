using ClientEcommerce.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ClientEcommerce.API.Services
{
    public class WhatsappService : IWhatsappService
    {
        private readonly AppDbContext _context;

        public WhatsappService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAllowedWhatsappUser(string phoneNumber)
        {
            return await _context.AllowedWhatsappUsers
                .AnyAsync(x => x.PhoneNumber == phoneNumber && x.IsActive);
        }

        public async Task<List<string>> GetAdminWhatsappNumbers()
        {
            return await _context.NotificationRecipients
                .Where(x => x.Role == "Admin" && x.IsActive)
                .Select(x => x.PhoneNumber)
                .ToListAsync();
        }

        public async Task SendWhatsapp(string to, string message)
        {
            var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            var from = Environment.GetEnvironmentVariable("TWILIO_WHATSAPP_FROM");

            if (string.IsNullOrWhiteSpace(accountSid) ||
                string.IsNullOrWhiteSpace(authToken) ||
                string.IsNullOrWhiteSpace(from))
            {
                Console.WriteLine("[WhatsApp ERROR] Missing Twilio environment variables.");
                return;
            }

            string toNormalized;
            string fromNormalized;

            try
            {
                toNormalized = NormalizeWhatsappAddress(to);
                fromNormalized = NormalizeWhatsappAddress(from);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp ERROR] Invalid number format. To={to}, Error={ex.Message}");
                return;
            }

            try
            {
                TwilioClient.Init(accountSid, authToken);

                var result = await MessageResource.CreateAsync(
                    from: new PhoneNumber(fromNormalized),
                    to: new PhoneNumber(toNormalized),
                    body: message
                );

                Console.WriteLine($"[WhatsApp SUCCESS] Sent to {toNormalized}, SID={result.Sid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp ERROR] Failed sending message. {ex.Message}");
            }
        }

        private static string NormalizeWhatsappAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number empty");

            var cleaned = new string(value
                .Replace("whatsapp:", "", StringComparison.OrdinalIgnoreCase)
                .Where(c => char.IsDigit(c) || c == '+')
                .ToArray());

            if (cleaned.StartsWith("+") && cleaned.Length >= 11)
                return $"whatsapp:{cleaned}";

            if (cleaned.Length == 10)
                return $"whatsapp:+91{cleaned}";

            if (cleaned.Length == 12 && cleaned.StartsWith("91"))
                return $"whatsapp:+{cleaned}";

            throw new ArgumentException($"Invalid phone number format: {cleaned}");
        }
    }
}