using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;
using System.Security.Claims;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int UserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // ================= ADD TO CART =================
        [HttpPost("add")]
        public IActionResult AddToCart(AddToCartDto dto)
        {
            _cartService.AddToCart(UserId, dto);
            return Ok();
        }

        // ================= GET CART =================
        [HttpGet]
        public IActionResult GetCart()
        {
            return Ok(_cartService.GetCart(UserId));
        }

        // ================= REMOVE ITEM =================
        [HttpDelete("remove/{productVariantId}")]
        public IActionResult RemoveItem(int productVariantId)
        {
            _cartService.RemoveItem(UserId, productVariantId);
            return Ok();
        }

        // ================= UPDATE QUANTITY =================
        [HttpPut("update")]
        public IActionResult UpdateQuantity(UpdateCartQuantityDto dto)
        {
            _cartService.UpdateQuantity(UserId, dto);
            return Ok();
        }

        // ================= CLEAR CART ✅ =================
        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart(UserId);
            return Ok();
        }
    }
}
