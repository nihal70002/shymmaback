using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientEcommerce.API.Services
{
    public interface IWhatsappService
    {
        Task<bool> IsAllowedWhatsappUser(string phoneNumber);

        Task<List<string>> GetAdminWhatsappNumbers();

        Task SendWhatsapp(string to, string message);
    }
}