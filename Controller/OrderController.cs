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
        [Authorize(Roles = "User")]
        public IActionResult PlaceOrder([FromBody] PlaceOrderByCustomerDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _orderService.PlaceOrder(userId, dto);
            return Ok(new { message = "Order placed successfully" });
        }

        // ==========================
        // USER: MY ORDERS
        // ==========================
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public IActionResult GetMyOrders()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(_orderService.GetOrdersForUser(userId));
        }

        // ==========================
        // USER: ORDER DETAILS
        // ==========================
        [HttpGet("my/{orderId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyOrderDetails(int orderId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _orderService.GetMyOrderDetailsAsync(userId, orderId);
            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
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
        // ADMIN: GET ORDER DETAILS
        // ==========================
        [HttpGet("{orderId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetOrderById(int orderId)
        {
            return Ok(_orderService.GetOrderById(orderId));
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
