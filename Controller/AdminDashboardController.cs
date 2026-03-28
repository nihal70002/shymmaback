using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.Services;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            var summary = _dashboardService.GetSummary();
            return Ok(summary);
        }
    }
}
