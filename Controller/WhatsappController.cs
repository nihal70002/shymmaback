using ClientEcommerce.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsappController : ControllerBase
    {
        private readonly IWhatsappService _whatsappService;

        public WhatsappController(IWhatsappService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        [HttpPost("webhook")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ReceiveMessage(
            [FromForm] string Body,
            [FromForm] string From)
        {
            Console.WriteLine("WHATSAPP WEBHOOK HIT");

            try
            {
                if (string.IsNullOrWhiteSpace(Body) ||
                    string.IsNullOrWhiteSpace(From))
                {
                    return Content("<Response></Response>", "text/xml");
                }

                var sender = From.Replace("whatsapp:", "");

                // allow only Indian numbers
                if (!sender.StartsWith("+91"))
                {
                    Console.WriteLine("Blocked non-Indian number");
                    return Content("<Response></Response>", "text/xml");
                }

                // check allowed sender from DB
                var isAllowed =
                    await _whatsappService.IsAllowedWhatsappUser(sender);

                if (!isAllowed)
                {
                    Console.WriteLine("Unauthorized sender");
                    return Content("<Response></Response>", "text/xml");
                }

                // get admin numbers from DB
                var admins =
                    await _whatsappService.GetAdminWhatsappNumbers();

                foreach (var admin in admins)
                {
                    await _whatsappService.SendWhatsapp(
                        admin,
                        $"Alert from {sender}\n\n{Body}"
                    );
                }

                Console.WriteLine("Alert forwarded to admins");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook error: {ex.Message}");
            }

            return Content("<Response></Response>", "text/xml");
        }
    }
}