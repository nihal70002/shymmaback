using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly ICloudinaryService _cloudinary;

        public CategoriesController(
            ICategoryService service,
            ICloudinaryService cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        // 🔓 Public (for users)
        [HttpGet]
        public IActionResult GetActiveCategories()
        {
            return Ok(_service.GetAll(admin: false));
        }

        // 🔐 Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAllForAdmin()
        {
            return Ok(_service.GetAll(admin: true));
        }

        // 🔐 Admin Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto)
        {
            try
            {
                string? imageUrl = null;

                if (dto.Image != null)
                {
                    imageUrl = await _cloudinary.UploadImageAsync(dto.Image);
                }

                _service.Create(dto, imageUrl);

                return Ok(new { message = "Category created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔐 Admin Update
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto dto)
        {
            try
            {
                string? imageUrl = null;

                if (dto.Image != null)
                {
                    imageUrl = await _cloudinary.UploadImageAsync(dto.Image);
                }

                _service.Update(id, dto, imageUrl);

                return Ok(new { message = "Category updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔐 Admin Delete
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.Delete(id);
                return Ok("Category deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔓 Public Get by Slug
        [HttpGet("{slug}")]
        public IActionResult GetBySlug(string slug)
        {
            var category = _service.GetCategoryWithChildren(slug);

            if (category == null)
                return NotFound();

            return Ok(category);
        }
    }
}