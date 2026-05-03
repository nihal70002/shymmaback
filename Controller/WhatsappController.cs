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
        private readonly ILogger<WhatsappController> _logger;

        public WhatsappController(IWhatsappService whatsappService, ILogger<WhatsappController> logger)
        {
            _whatsappService = whatsappService;
            _logger = logger;
        }

        [HttpPost("webhook")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ReceiveMessage(
            [FromForm] string Body,
            [FromForm] string From)
        {
            _logger.LogInformation("WhatsApp webhook hit");

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
                    _logger.LogWarning("Blocked non-Indian number: {Sender}", sender);
                    return Content("<Response></Response>", "text/xml");
                }

                // check allowed sender from DB
                var isAllowed =
                    await _whatsappService.IsAllowedWhatsappUser(sender);

                if (!isAllowed)
                {
                    _logger.LogWarning("Unauthorized WhatsApp sender: {Sender}", sender);
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

                _logger.LogInformation("WhatsApp alert forwarded to {AdminCount} admins", admins.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp webhook error");
            }

            return Content("<Response></Response>", "text/xml");
        }
    }
}