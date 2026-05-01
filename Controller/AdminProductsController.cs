using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;
using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ADD PRODUCT
        [HttpPost]
        public IActionResult Create(AdminCreateProductDto dto)
        {
            _productService.CreateProduct(dto);
            return Ok("Product created");
        }

        // LIST PRODUCTS
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_productService.GetAllProducts());
        }

        // TOGGLE ACTIVE
        [HttpPut("{productId}/toggle")]
        public IActionResult Toggle(int productId)
        {
            _productService.ToggleProduct(productId);
            return Ok("Product status updated");
        }

        // UPDATE STOCK
        [HttpPut("variant/{variantId}/stock")]
        public IActionResult UpdateStock(int variantId, [FromQuery] int stock)
        {
            _productService.UpdateVariantStock(variantId, stock);
            return Ok("Stock updated");
        }
        [HttpPut("{productId}")]
        public IActionResult UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            _productService.UpdateProduct(productId, dto);
            return Ok("Product updated successfully");
        }
        [HttpPut("variant/{variantId}")]
        public IActionResult UpdateVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            try
            {
                _productService.UpdateProductVariant(variantId, dto);
                return Ok("Variant updated successfully");
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{productId}/variants")]
        public IActionResult UpsertVariants(int productId, List<AdminUpsertProductVariantDto> variants)
        {
            try
            {
                _productService.UpsertProductVariants(productId, variants);
                return Ok("Variants updated successfully");
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("{productId}/variant")]
        public IActionResult AddVariant(
    int productId,
    AdminCreateProductVariantDto dto)
        {
            try
            {
                _productService.AddProductVariant(productId, dto);
                return Ok("Variant added");
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{productId}")]
        public IActionResult GetProduct(int productId)
        {
            var product = _productService.GetProductById(productId);

            if (product == null)
                return NotFound("Product not found");

            return Ok(product);
        }


        [HttpGet("low-stock")]
        public IActionResult LowStock([FromQuery] int threshold = 5)
        {
            return Ok(_productService.GetLowStockVariants(threshold));
        }
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            await _productService.DeleteProductAsync(productId);
            return Ok();
        }





    }
}
