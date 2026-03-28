using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpPost("bulk")]
        public IActionResult BulkCreateProducts(
    [FromBody] List<AdminBulkCreateProductDto> products)
        {
            _productService.BulkCreate(products);
            return Ok(new { message = "Bulk products created successfully" });
        }

        // ===========================
        // LIST PRODUCTS (USER) - PAGINATED
        // GET: api/products?page=1&pageSize=12
        // ===========================



        [HttpGet]
        public IActionResult GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] List<int>? categoryIds = null,
    [FromQuery] int? brandId = null,
    [FromQuery] string? search = null)
        {
            var result = _productService.GetProducts(
                page,
                pageSize,
                categoryIds,
                brandId,
                search);

            return Ok(result);
        }


        // ===========================
        // PRODUCT DETAILS (USER)
        // GET: api/products/5
        // ===========================
        [HttpGet("{productId:int}")]
        public IActionResult GetById(int productId)
        {
            var product = _productService.GetProductById(productId);

            if (product == null)
                return NotFound("Product not found");

            return Ok(product);
        }
    }
}
