using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;


        public AdminController(IUserService userService, IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }

        // ==========================
        // ADMIN: CREATE USER
        // ==========================
        [HttpPost("users")]
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            _userService.CreateUser(dto);

            return Ok(new
            {
                message = "User created successfully"
            });
        }

        // ==========================
        // ADMIN: GET ALL USERS
        // ==========================
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        // ==========================
        // ADMIN: USER DETAILS
        // ==========================
        [HttpGet("users/{userId}")]
        public IActionResult GetUserDetails(int userId)
        {
            var user = _userService.GetUserDetails(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }
        [HttpGet("orders")]
       
        public async Task<IActionResult> GetOrders(int page = 1, int pageSize = 8, string? status = null)
        {
            var result = await _orderService.GetAdminOrders(page, pageSize, status);

            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount
            });
        }

    }
}
