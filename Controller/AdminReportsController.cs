using Microsoft.AspNetCore.Mvc;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IAdminReportService _service;

        public AdminReportsController(IAdminReportService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            return Ok(_service.GetSalesSummary());
        }

        [HttpGet("monthly")]
        public IActionResult GetMonthlySales([FromQuery] int year)
        {
            return Ok(_service.GetMonthlySales(year));
        }

        [HttpGet("top-products")]
        public IActionResult GetTopProducts()
        {
            return Ok(_service.GetTopProducts());
        }
        [HttpGet("top-customers")]
        public IActionResult GetTopCustomers()
        {
            return Ok(_service.GetTopCustomers());
        }

        [HttpGet("customer-interest/{userId}")]
        public IActionResult GetCustomerInterest(int userId)
        {
            return Ok(_service.GetCustomerProductInterest(userId));
        }

    }
}
