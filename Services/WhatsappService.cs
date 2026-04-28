using ClientEcommerce.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var toNormalized = NormalizeWhatsappAddress(to);
            var fromNormalized = NormalizeWhatsappAddress(from);

            TwilioClient.Init(accountSid, authToken);

            await MessageResource.CreateAsync(
                from: new PhoneNumber(fromNormalized),
                to: new PhoneNumber(toNormalized),
                body: message
            );
        }

        private static string NormalizeWhatsappAddress(string value)
        {
            var trimmed = value.Trim();
            return trimmed.StartsWith("whatsapp:") ? trimmed : $"whatsapp:{trimmed}";
        }
    }
}