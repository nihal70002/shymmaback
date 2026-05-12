using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;
using System.Security.Claims;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ==========================
        // USER: PLACE ORDER
        // ==========================
        [HttpPost]
        [Authorize(Roles = "User,Customer,Admin")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderByCustomerDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _orderService.PlaceOrder(userId, dto);
                return Ok(new { message = "Order placed successfully" });
            }
            catch (Exception ex)
            {
                // Log the detailed error for debugging
                Console.WriteLine($"PlaceOrder Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw to be handled by GlobalExceptionMiddleware
            }
        }

        // ==========================
        // USER: MY ORDERS
        // ==========================
        [HttpGet("my")]
        [Authorize(Roles = "User,Customer,Admin")]
        public IActionResult GetMyOrders()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(_orderService.GetOrdersForUser(userId));
        }

        // ==========================
        // USER: ORDER DETAILS
        // ==========================
        [HttpGet("my/{orderId}")]
        [Authorize(Roles = "User,Customer,Admin")]
        public async Task<IActionResult> GetMyOrderDetails(int orderId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _orderService.GetMyOrderDetailsAsync(userId, orderId);
            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        // ==========================
        // USER: CANCEL ORDER
        // ==========================
        [HttpPost("my/{orderId}/cancel")]
        [Authorize(Roles = "User,Customer,Admin")]
        public IActionResult CancelMyOrder(int orderId, [FromBody] string reason)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _orderService.CancelCustomerOrder(userId, orderId, reason);
            return Ok(new { message = "Order cancelled successfully" });
        }

        // ==========================
        // ADMIN: CONFIRM ORDER
        // ==========================
        [HttpPost("{orderId}/confirm")]
        [Authorize(Roles = "Admin")]
        public IActionResult ConfirmOrder(int orderId)
        {
            _orderService.ConfirmOrder(orderId);
            return Ok(new { message = "Order confirmed" });
        }

        // ==========================
        // ADMIN: REJECT ORDER
        // ==========================

       


        // ==========================
        // ADMIN: CANCEL ORDER
        // ==========================
        [HttpPost("{orderId}/cancel")]
        [Authorize(Roles = "Admin")]
        public IActionResult CancelOrder(int orderId, [FromBody] string reason)
        {
            _orderService.CancelOrder(orderId, reason);
            return Ok(new { message = "Order cancelled" });
        }

        // ==========================
        // ADMIN: DISPATCH ORDER
        // ==========================
        [HttpPost("{orderId}/dispatch")]
        [Authorize(Roles = "Admin")]
        public IActionResult DispatchOrder(int orderId)
        {
            _orderService.DispatchOrder(orderId);
            return Ok(new { message = "Order dispatched" });
        }

        // ==========================
        // ADMIN: DELIVER ORDER
        // ==========================
        [HttpPost("{orderId}/deliver")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeliverOrder(int orderId)
        {
            _orderService.DeliverOrder(orderId);
            return Ok(new { message = "Order delivered" });
        }

        // ==========================
        // ADMIN: RECENT ORDERS
        // ==========================
        [HttpGet("admin/recent")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetRecentOrders([FromQuery] int count = 10)
        {
            return Ok(_orderService.GetRecentOrders(count));
        }
    }
}
