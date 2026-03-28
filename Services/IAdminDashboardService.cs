using ClientEcommerce.API.DTOs.Admin;

namespace ClientEcommerce.API.Services
{
    public interface IAdminDashboardService
    {
        AdminDashboardSummaryDto GetSummary();
    }
}
