using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientEcommerce.API.Services
{
    public class CategoryService : ICategoryService
    {
        private const int MaxCategoryDepth = 3;
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
                .OrderBy(c => c.ParentCategoryId == null ? 0 : 1)
                .ThenBy(c => c.ParentCategoryId)
                .ThenBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    IsActive = c.IsActive,
                    ParentCategoryId = c.ParentCategoryId,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder
                })
                .ToList();
        }

        public void Create(CreateCategoryDto dto, string? imageUrl)
        {
            if (_context.Categories.Any(c => c.Name == dto.Name))
                throw new BadRequestException("Category already exists");

            if (dto.ParentCategoryId.HasValue)
            {
                var parentDepth = GetCategoryDepth(dto.ParentCategoryId.Value);
                if (parentDepth >= MaxCategoryDepth)
                    throw new BadRequestException("Only 3-level category hierarchy allowed");
            }

            var displayOrder = _context.Categories
                .Where(c => c.ParentCategoryId == dto.ParentCategoryId)
                .Select(c => (int?)c.DisplayOrder)
                .Max() ?? 0;

            _context.Categories.Add(new Category
            {
                Name = dto.Name,
                Slug = dto.Name.ToLower().Replace(" ", "-"),
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = true,
                ImageUrl = imageUrl,
                DisplayOrder = displayOrder + 1
            });

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            if (_context.Categories.Any(c => c.ParentCategoryId == id))
                throw new BadRequestException("Cannot delete category with subcategories");

            if (_context.Products.Any(p => p.CategoryId == id))
                throw new BadRequestException("Cannot delete category assigned to products");

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
            var decodedSlug = System.Web.HttpUtility.UrlDecode(slug);

            var categoryDto = _context.Categories
                .Where(c => c.Slug == decodedSlug && c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    IsActive = c.IsActive,
                    ParentCategoryId = c.ParentCategoryId,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder,
                    SubCategories = c.SubCategories
                        .Where(sc => sc.IsActive)
                        .OrderBy(sc => sc.DisplayOrder)
                        .ThenBy(sc => sc.Id)
                        .Select(sc => new CategoryDto
                        {
                            Id = sc.Id,
                            Name = sc.Name,
                            Slug = sc.Slug,
                            IsActive = sc.IsActive,
                            ParentCategoryId = sc.ParentCategoryId,
                            ImageUrl = sc.ImageUrl,
                            DisplayOrder = sc.DisplayOrder,
                            SubCategories = sc.SubCategories
                                .Where(gc => gc.IsActive)
                                .OrderBy(gc => gc.DisplayOrder)
                                .ThenBy(gc => gc.Id)
                                .Select(gc => new CategoryDto
                                {
                                    Id = gc.Id,
                                    Name = gc.Name,
                                    Slug = gc.Slug,
                                    IsActive = gc.IsActive,
                                    ParentCategoryId = gc.ParentCategoryId,
                                    ImageUrl = gc.ImageUrl,
                                    DisplayOrder = gc.DisplayOrder
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefault();

            return categoryDto;
        }

        public void Update(int id, UpdateCategoryDto dto, string? newImageUrl)
        {
            var category = _context.Categories
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                throw new NotFoundException("Category not found");

            if (dto.ParentCategoryId.HasValue)
            {
                if (dto.ParentCategoryId == id)
                    throw new BadRequestException("Category cannot be its own parent");

                var parent = _context.Categories
                    .FirstOrDefault(c => c.Id == dto.ParentCategoryId.Value);

                if (parent == null)
                    throw new NotFoundException("Parent category not found");

                if (IsDescendant(dto.ParentCategoryId.Value, id))
                    throw new BadRequestException("Category cannot be moved below its own child");
            }

            var newParentDepth = dto.ParentCategoryId.HasValue
                ? GetCategoryDepth(dto.ParentCategoryId.Value)
                : 0;
            var subtreeHeight = GetSubtreeHeight(id);

            if (newParentDepth + subtreeHeight > MaxCategoryDepth)
                throw new BadRequestException("Only 3-level category hierarchy allowed");

            var oldParentCategoryId = category.ParentCategoryId;

            category.Name = dto.Name;
            category.Slug = dto.Name.ToLower().Replace(" ", "-");
            category.ParentCategoryId = dto.ParentCategoryId;

            if (oldParentCategoryId != dto.ParentCategoryId)
            {
                var displayOrder = _context.Categories
                    .Where(c => c.Id != id && c.ParentCategoryId == dto.ParentCategoryId)
                    .Select(c => (int?)c.DisplayOrder)
                    .Max() ?? 0;

                category.DisplayOrder = displayOrder + 1;
            }

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

        private int GetCategoryDepth(int categoryId)
        {
            var depth = 0;
            int? currentId = categoryId;

            while (currentId.HasValue)
            {
                var category = _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == currentId.Value)
                    .Select(c => new { c.Id, c.ParentCategoryId })
                    .FirstOrDefault();

                if (category == null)
                    throw new NotFoundException("Category not found");

                depth++;
                currentId = category.ParentCategoryId;

                if (depth > MaxCategoryDepth)
                    break;
            }

            return depth;
        }

        private int GetSubtreeHeight(int categoryId)
        {
            var childIds = _context.Categories
                .AsNoTracking()
                .Where(c => c.ParentCategoryId == categoryId)
                .Select(c => c.Id)
                .ToList();

            if (!childIds.Any())
                return 1;

            return 1 + childIds.Max(GetSubtreeHeight);
        }

        private bool IsDescendant(int possibleDescendantId, int categoryId)
        {
            int? currentId = possibleDescendantId;

            while (currentId.HasValue)
            {
                if (currentId.Value == categoryId)
                    return true;

                currentId = _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == currentId.Value)
                    .Select(c => c.ParentCategoryId)
                    .FirstOrDefault();
            }

            return false;
        }

        public void Reorder(ReorderCategoriesDto dto)
        {
            if (dto.Categories.Count == 0)
                throw new BadRequestException("No categories supplied");

            var ids = dto.Categories.Select(c => c.Id).Distinct().ToList();
            var categories = _context.Categories
                .Where(c => ids.Contains(c.Id))
                .ToList();

            if (categories.Count != ids.Count)
                throw new BadRequestException("One or more categories were not found");

            var expectedParentId = categories.First().ParentCategoryId;

            if (categories.Any(c => c.ParentCategoryId != expectedParentId))
                throw new BadRequestException("Only categories with the same parent can be reordered together");

            foreach (var item in dto.Categories)
            {
                var category = categories.First(c => c.Id == item.Id);

                if (category.ParentCategoryId != item.ParentCategoryId)
                    throw new BadRequestException("Category parent mismatch");

                category.DisplayOrder = item.DisplayOrder;
            }

            _context.SaveChanges();
        }
    }
}
