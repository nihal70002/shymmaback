using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientEcommerce.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CategoryDto> GetAll(bool admin)
        {
            var query = _context.Categories.AsQueryable();

            if (!admin)
                query = query.Where(c => c.IsActive);

            return query
    .OrderBy(c => c.Name)
    .Select(c => new CategoryDto
    {
        Id = c.Id,
        Name = c.Name,
        IsActive = c.IsActive,
        ParentCategoryId = c.ParentCategoryId,
        ImageUrl = c.ImageUrl // 🔥 ADD THIS
    })
    .ToList();

        }


        public void Create(CreateCategoryDto dto, string? imageUrl)
        {
            if (_context.Categories.Any(c => c.Name == dto.Name))
                throw new Exception("Category already exists");

            _context.Categories.Add(new Category
            {
                Name = dto.Name,
                Slug = dto.Name.ToLower().Replace(" ", "-"),
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = true,
                ImageUrl = imageUrl
            });

            _context.SaveChanges();
        }



        public void Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                throw new Exception("Category not found");

            // 1️⃣ Block if has subcategories
            if (_context.Categories.Any(c => c.ParentCategoryId == id))
                throw new Exception("Cannot delete category with subcategories");

            // 2️⃣ Block if assigned to products
            if (_context.Products.Any(p => p.CategoryId == id))
                throw new Exception("Cannot delete category assigned to products");

            _context.Categories.Remove(category);
            _context.SaveChanges();
        }

        public Category? GetBySlugWithChildren(string slug)
        {
            return _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.Slug == slug && c.IsActive);
        }


        public CategoryDto? GetCategoryWithChildren(string slug)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.Slug == slug && c.IsActive);

            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ImageUrl = category.ImageUrl,   // 🔥 ADD THIS
                SubCategories = category.SubCategories
                    .Where(sc => sc.IsActive)
                    .Select(sc => new CategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Slug = sc.Slug,
                        IsActive = sc.IsActive,
                        ParentCategoryId = sc.ParentCategoryId,
                        ImageUrl = sc.ImageUrl
                    }).ToList()
            };
        }

        public void Update(int id, UpdateCategoryDto dto, string? newImageUrl)
        {
            var category = _context.Categories
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                throw new Exception("Category not found");

            // Validate parent
            if (dto.ParentCategoryId.HasValue)
            {
                if (dto.ParentCategoryId == id)
                    throw new Exception("Category cannot be its own parent");

                var parent = _context.Categories
                    .FirstOrDefault(c => c.Id == dto.ParentCategoryId.Value);

                if (parent == null)
                    throw new Exception("Parent category not found");

                if (parent.ParentCategoryId != null)
                    throw new Exception("Only 2-level hierarchy allowed");
            }

            var hasChildren = _context.Categories
                .Any(c => c.ParentCategoryId == id);

            if (hasChildren && dto.ParentCategoryId != null)
                throw new Exception("Cannot move main category that has subcategories");

            category.Name = dto.Name;
            category.ParentCategoryId = dto.ParentCategoryId;

            // ❌ DO NOT TOUCH IsActive HERE

            if (dto.RemoveImage)
            {
                category.ImageUrl = null;
            }

            if (newImageUrl != null)
            {
                category.ImageUrl = newImageUrl;
            }

            _context.SaveChanges();
        }

    }
}