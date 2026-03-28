using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _service;

        public BrandController(IBrandService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetBrands()
        {
            return Ok(_service.GetBrands());
        }

        [HttpPost]
        public IActionResult AddBrand([FromBody] CreateBrandDto dto)
        {
            _service.CreateBrand(dto);
            return Ok(new { message = "Brand created successfully" });
        }

    }
}
