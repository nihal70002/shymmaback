using ClientEcommerce.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                System.Console.WriteLine($"[WhatsApp disabled] Missing TWILIO_ACCOUNT_SID/TWILIO_AUTH_TOKEN/TWILIO_WHATSAPP_FROM. To={to}, Message={message}");
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
                System.Console.WriteLine($"[WhatsApp invalid number] To={to}, From={from}. {ex.Message}");
                throw;
            }

            try
            {
                TwilioClient.Init(accountSid, authToken);

                var result = await MessageResource.CreateAsync(
                    from: new PhoneNumber(fromNormalized),
                    to: new PhoneNumber(toNormalized),
                    body: message
                );

                System.Console.WriteLine($"[WhatsApp sent] Sid={result.Sid}, To={toNormalized}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[WhatsApp send failed] To={toNormalized}, From={fromNormalized}. {ex}");
                throw;
            }
        }

        private static string NormalizeWhatsappAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number is empty");

            var trimmed = value.Trim();
            if (trimmed.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed.Substring("whatsapp:".Length);

            // Keep only digits and '+'
            var sb = new StringBuilder();
            foreach (var ch in trimmed)
            {
                if (char.IsDigit(ch) || ch == '+')
                    sb.Append(ch);
            }

            var cleaned = sb.ToString();

            // If already E.164
            if (cleaned.StartsWith("+") && cleaned.Length >= 11 && cleaned.Length <= 16)
                return $"whatsapp:{cleaned}";

            // India defaults (per your requirement)
            var digitsOnly = new string(cleaned.Where(char.IsDigit).ToArray());

            // 10-digit mobile => +91
            if (digitsOnly.Length == 10)
                return $"whatsapp:+91{digitsOnly}";

            // 91xxxxxxxxxx (12 digits) => +91...
            if (digitsOnly.Length == 12 && digitsOnly.StartsWith("91"))
                return $"whatsapp:+{digitsOnly}";

            // 0xxxxxxxxxx (11 digits) => +91...
            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("0"))
                return $"whatsapp:+91{digitsOnly.Substring(1)}";

            throw new ArgumentException($"Invalid phone number format after cleaning: '{digitsOnly}'");
        }
    }
}