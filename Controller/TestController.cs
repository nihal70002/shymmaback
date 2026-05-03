using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    [Authorize(Roles = "Admin")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is running", timestamp = DateTime.UtcNow });
        }
    }
}
