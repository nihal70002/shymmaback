using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ClientEcommerce.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        private const string ProductCacheVersionKey = "products_cache_version";

        public ProductService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IEnumerable<ProductListDto> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Category)
.Include(p => p.Brand)     // ✅ ADD
.Include(p => p.Variants)
.Include(p => p.Images)


                .OrderByDescending(p => p.Id)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    NameArabic = p.NameArabic,
                    ProductCode = p.ProductCode,
                    CategoryId = p.CategoryId,
                    Description = p.Description, // admin products description fix


                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,                    // ✅ ADD
                    BrandName = p.Brand.BrandName,
                    ImageUrls = p.Images
    .OrderByDescending(i => i.IsPrimary)
    .Select(i => i.ImageUrl)
    .ToList(),

                    PrimaryImageUrl = p.Images
    .Where(i => i.IsPrimary)
    .Select(i => i.ImageUrl)
    .FirstOrDefault(),

                    IsActive = p.IsActive,
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        ProductCode = v.ProductCode,
                        Price = v.Price,
                        Stock = v.Stock
                    }).ToList()
                })
                .ToList();
        }


        // ===========================
        // CACHE VERSION HELPERS
        // ===========================
        private int GetCacheVersion()
        {
            var version = _cache.GetString(ProductCacheVersionKey);
            if (version == null)
            {
                _cache.SetString(ProductCacheVersionKey, "1");
                return 1;
            }
            return int.Parse(version);
        }

        private void IncrementCacheVersion()
        {
            var version = GetCacheVersion() + 1;
            _cache.SetString(ProductCacheVersionKey, version.ToString());
        }

        // ===========================
        // USER – LIST PRODUCTS (PAGED)
        // ===========================




        public PagedResponseDto<ProductListDto> GetProducts(
    int page,
    int pageSize,
    List<int>? categoryIds,
    int? brandId,
    string? search)
        {
            int version = GetCacheVersion();

            var categoryKey = categoryIds != null && categoryIds.Any()
                ? string.Join("-", categoryIds.OrderBy(x => x))
                : "none";

            string cacheKey =
                $"products_v{version}_page_{page}_{pageSize}_cats_{categoryKey}_brand_{brandId}_search_{search}";

            var cachedData = _cache.GetString(cacheKey);
            if (cachedData != null)
            {
                Console.WriteLine("PRODUCTS → REDIS CACHE HIT");
                return JsonSerializer.Deserialize<PagedResponseDto<ProductListDto>>(cachedData)!;
            }

            Console.WriteLine("PRODUCTS → DB HIT");

            var query = _context.Products
                .Where(p => p.IsActive);

            // 🔹 CATEGORY FILTER (2-level hierarchy support)
            if (categoryIds != null && categoryIds.Any())
            {
                var selectedCategories = _context.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToList();

                var mainCategoryIds = selectedCategories
                    .Where(c => c.ParentCategoryId == null)
                    .Select(c => c.Id)
                    .ToList();

                var subCategoryIds = selectedCategories
                    .Where(c => c.ParentCategoryId != null)
                    .Select(c => c.Id)
                    .ToList();

                var childrenOfMain = _context.Categories
                    .Where(c => c.ParentCategoryId.HasValue &&
                                mainCategoryIds.Contains(c.ParentCategoryId.Value))
                    .Select(c => c.Id)
                    .ToList();

                var finalCategoryIds = subCategoryIds
                    .Concat(childrenOfMain)
                    .Distinct()
                    .ToList();

                query = query.Where(p => finalCategoryIds.Contains(p.CategoryId));
            }

            // 🔹 BRAND FILTER
            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            // 🔹 SEARCH FILTER (English + Arabic)
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.NameArabic.Contains(search)
                );

            // 🔹 TOTAL COUNT (before pagination)
            var totalCount = query.Count();

            // 🔹 ORDERING
            query = query.OrderByDescending(p => p.Id);

            // 🔹 PAGINATION + PROJECTION
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    NameArabic = p.NameArabic,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.BrandName,
                    Description = p.Description,
                    PrimaryImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        Price = v.Price,
                        Stock = v.Stock
                    }).ToList()
                })
                .ToList();

            // 🔹 BUILD RESPONSE
            var result = new PagedResponseDto<ProductListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                HasMore = page * pageSize < totalCount
            };

            // 🔹 CACHE RESPONSE
            _cache.SetString(
                cacheKey,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                });

            return result;
        }



        // ===========================
        // USER – PRODUCT DETAILS
        // ===========================

        public ProductDetailDto GetProductById(int productId)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.Id == productId && p.IsActive);

            if (product == null) return null;

            return new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                NameArabic = product.NameArabic,
                CategoryId = product.CategoryId,
                ProductCode = product.ProductCode,
                CategoryName = product.Category.Name,
                Description = product.Description,

                ImageUrls = product.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .ToList(),

                PrimaryImageUrl = product.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),

                // 🔥 UPDATED VARIANT MAPPING
                Sizes = product.Variants.Select(v => new ProductVariantDto
                {
                    VariantId = v.Id,
                    Size = v.Size,

                    Class = v.Class,
                    Style = v.Style,
                    Material = v.Material,
                    Color = v.Color,

                    Price = v.Price,
                    AvailableStock = v.Stock
                }).ToList()
            };
        }





        // ===========================
        // ADMIN – CREATE PRODUCT
        // ===========================

        public void CreateProduct(AdminCreateProductDto dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1️⃣ Validations
                var category = _context.Categories
    .FirstOrDefault(c => c.Id == dto.CategoryId);

                if (category == null)
                    throw new ValidationException("Invalid category selected");

                // 🔥 IMPORTANT: Product must belong to subcategory only
                if (category.ParentCategoryId == null)
                    throw new ValidationException("Product must belong to subcategory only");

                if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                    throw new ValidationException("Invalid Brand");

                if (dto.ImageUrls.Count > 5)
                    throw new ValidationException("Maximum 5 images allowed per product");

                var duplicateCombinations = dto.Variants
    .GroupBy(v => new
    {
        Class = v.Class.Trim().ToLower(),
        Style = v.Style.Trim().ToLower(),
        Material = v.Material.Trim().ToLower(),
        Color = v.Color.Trim().ToLower(),
        Size = v.Size.Trim().ToLower()
    })
    .Where(g => g.Count() > 1)
    .ToList();

                if (duplicateCombinations.Any())
                    throw new ValidationException("Duplicate variant combination detected.");


                foreach (var v in dto.Variants)
                {
                    var sku = v.ProductCode?.Trim();

                    if (string.IsNullOrWhiteSpace(sku))
                        throw new ValidationException("SKU / ProductCode is required");

                    bool skuExists = _context.ProductVariants.Any(pv =>
                        pv.ProductCode.ToLower() == sku.ToLower());

                    if (skuExists)
                        throw new ValidationException($"SKU already exists: {sku}");
                }

                // 2️⃣ Create Product
                var product = new Product
                {
                    Name = dto.Name,
                    NameArabic = dto.NameArabic,
                    CategoryId = dto.CategoryId,
                    BrandId = dto.BrandId,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                // 3️⃣ Add Images
                for (int i = 0; i < dto.ImageUrls.Count; i++)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = dto.ImageUrls[i],
                        IsPrimary = i == 0
                    });
                }

                // 4️⃣ Add Variants
                foreach (var v in dto.Variants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        Class = v.Class?.Trim(),
                        Style = v.Style?.Trim(),
                        Material = v.Material?.Trim(),
                        Color = v.Color?.Trim(),
                        Size = v.Size?.Trim(),
                        ProductCode = v.ProductCode?.Trim(),
                        Price = v.Price,
                        Stock = v.Stock
                    });
                }


                _context.SaveChanges();

                // ✅ 1. Commit DB transaction
                transaction.Commit();

                // ✅ 2. Invalidate product cache
                IncrementCacheVersion();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }









        // ===========================
        // ADMIN – UPDATE PRODUCT
        // ===========================
        public void UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new ValidationException("Product not found");

            if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                throw new ValidationException("Invalid Brand");

            var category = _context.Categories
     .FirstOrDefault(c => c.Id == dto.CategoryId);

            if (category == null)
                throw new ValidationException("Invalid Category");

            if (category.ParentCategoryId == null)
                throw new ValidationException("Product must belong to subcategory only");


            product.Name = dto.Name;
            product.NameArabic = dto.NameArabic;
            product.CategoryId = dto.CategoryId;

            product.BrandId = dto.BrandId;
            product.Description = dto.Description;
            _context.ProductImages.RemoveRange(
    _context.ProductImages.Where(i => i.ProductId == productId)
);

            if (dto.ImageUrls.Count > 5)
                throw new ValidationException("Maximum 5 images allowed");

            for (int i = 0; i < dto.ImageUrls.Count; i++)
            {
                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = dto.ImageUrls[i],
                    IsPrimary = i == 0
                });
            }


            _context.SaveChanges();
            IncrementCacheVersion();
        }


        // ===========================
        // ADMIN – TOGGLE PRODUCT
        // ===========================
        public void ToggleProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.IsActive = !product.IsActive;
            _context.SaveChanges();

            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – DELETE PRODUCT (SOFT)
        // ===========================
        public async Task DeleteProductAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            // 🔥 HARD DELETE
            _context.ProductVariants.RemoveRange(product.Variants);
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
        }


        // ===========================
        // ADMIN – UPDATE VARIANT
        // ===========================


        public void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                throw new ValidationException("Variant not found");

            // ---------- CLEAN VALUES ----------
            var classValue = dto.Class?.Trim();
            var style = dto.Style?.Trim();
            var material = dto.Material?.Trim();
            var color = dto.Color?.Trim();
            var size = dto.Size?.Trim();
            var sku = dto.ProductCode?.Trim();

            // ---------- REQUIRED FIELDS ----------
            if (string.IsNullOrWhiteSpace(size))
                throw new ValidationException("Size is required");

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU cannot be empty");

            // ---------- COMBINATION VALIDATION ----------
            bool combinationExists = _context.ProductVariants.Any(v =>
                v.ProductId == variant.ProductId &&
                (v.Class ?? "").ToLower() == (classValue ?? "").ToLower() &&
                (v.Style ?? "").ToLower() == (style ?? "").ToLower() &&
                (v.Material ?? "").ToLower() == (material ?? "").ToLower() &&
                (v.Color ?? "").ToLower() == (color ?? "").ToLower() &&
                (v.Size ?? "").ToLower() == size.ToLower() &&
                v.Id != variantId
            );

            if (combinationExists)
                throw new ValidationException("Variant combination already exists.");

            // ---------- SKU VALIDATION ----------
            bool skuExists = _context.ProductVariants.Any(v =>
                (v.ProductCode ?? "").ToLower() == sku.ToLower() &&
                v.Id != variantId
            );

            if (skuExists)
                throw new ValidationException($"SKU already exists: {sku}");

            // ---------- UPDATE ----------
            variant.Class = classValue;
            variant.Style = style;
            variant.Material = material;
            variant.Color = color;
            variant.Size = size;
            variant.Price = dto.Price;
            variant.ProductCode = sku;
            variant.Stock = dto.Stock;

            _context.SaveChanges();
            IncrementCacheVersion();
        }













        public void BulkCreate(List<AdminBulkCreateProductDto> products)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                foreach (var dto in products)
                {
                    // ---------- BASIC VALIDATION ----------
                    var category = _context.Categories
    .FirstOrDefault(c => c.Id == dto.CategoryId);

                    if (category == null)
                        throw new ValidationException($"Invalid category for product: {dto.Name}");

                    if (category.ParentCategoryId == null)
                        throw new ValidationException($"Product must belong to subcategory: {dto.Name}");




                    if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                        throw new ValidationException($"Invalid brand for product: {dto.Name}");

                    if (dto.ImageUrls?.Count > 5)
                        throw new ValidationException($"Maximum 5 images allowed for product: {dto.Name}");

                    // ---------- VARIANT COMBINATION VALIDATION ----------
                    var duplicateCombinations = dto.Variants
                        .GroupBy(v => new
                        {
                            Class = v.Class.Trim().ToLower(),
                            Style = v.Style.Trim().ToLower(),
                            Material = v.Material.Trim().ToLower(),
                            Color = v.Color.Trim().ToLower(),
                            Size = v.Size.Trim().ToLower()
                        })
                        .Where(g => g.Count() > 1)
                        .ToList();

                    if (duplicateCombinations.Any())
                        throw new ValidationException($"Duplicate variant combinations found in product: {dto.Name}");

                    // ---------- CREATE PRODUCT ----------
                    var product = new Product
                    {
                        Name = dto.Name,
                        CategoryId = dto.CategoryId,
                        BrandId = dto.BrandId,
                        Description = dto.Description,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Products.Add(product);
                    _context.SaveChanges(); // get product.Id

                    // ---------- ADD IMAGES ----------
                    if (dto.ImageUrls != null && dto.ImageUrls.Any())
                    {
                        bool isPrimary = true;

                        foreach (var imageUrl in dto.ImageUrls.Take(5))
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = imageUrl,
                                IsPrimary = isPrimary
                            });

                            isPrimary = false;
                        }
                    }

                    // ---------- ADD VARIANTS ----------
                    foreach (var v in dto.Variants)
                    {
                        var sku = v.ProductCode?.Trim();
                        if (string.IsNullOrWhiteSpace(sku))
                            throw new ValidationException($"SKU is required in product: {dto.Name}");

                        bool skuExists = _context.ProductVariants
                            .Any(pv => pv.ProductCode.ToLower() == sku.ToLower());

                        if (skuExists)
                            throw new ValidationException($"Duplicate SKU detected: {sku}");

                        _context.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            Class = v.Class.Trim(),
                            Style = v.Style.Trim(),
                            Material = v.Material.Trim(),
                            Color = v.Color.Trim(),
                            Size = v.Size.Trim(),
                            ProductCode = sku,
                            Price = v.Price,

                            LowStockThreshold = 10
                        });
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                IncrementCacheVersion();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }








        public void UpdateVariantStock(int variantId, int stock)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) throw new Exception("Variant not found");

            variant.Stock = stock;
            _context.SaveChanges();

            IncrementCacheVersion();
        }


        public void AddProductVariant(int productId, AdminCreateProductVariantDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new ValidationException("Product not found");

            // ---------- REQUIRED FIELD VALIDATION ----------
            var classValue = dto.Class?.Trim();
            var style = dto.Style?.Trim();
            var material = dto.Material?.Trim();
            var color = dto.Color?.Trim();
            var size = dto.Size?.Trim();



            if (string.IsNullOrWhiteSpace(size))
                throw new ValidationException("Size is required");

            if (dto.Stock < 0)
                throw new ValidationException("Stock cannot be negative");

            // ---------- COMBINATION VALIDATION ----------
            bool combinationExists = _context.ProductVariants.Any(v =>
                v.ProductId == productId &&
                v.Class.ToLower() == classValue.ToLower() &&
                v.Style.ToLower() == style.ToLower() &&
                v.Material.ToLower() == material.ToLower() &&
                v.Color.ToLower() == color.ToLower() &&
                v.Size.ToLower() == size.ToLower()
            );

            if (combinationExists)
                throw new ValidationException("This variant combination already exists");

            // ---------- SKU VALIDATION ----------
            var sku = dto.ProductCode?.Trim();

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU is required");

            bool skuExists = _context.ProductVariants.Any(v =>
                v.ProductCode.ToLower() == sku.ToLower());

            if (skuExists)
                throw new ValidationException($"SKU already exists: {sku}");

            // ---------- INSERT ----------
            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                Class = classValue,
                Style = style,
                Material = material,
                Color = color,
                Size = size,
                ProductCode = sku,
                Price = dto.Price,
                Stock = dto.Stock
            });

            _context.SaveChanges();
            IncrementCacheVersion();
        }








        // ===========================
        // ADMIN – LOW STOCK
        // ===========================
        public IEnumerable<LowStockVariantDto> GetLowStockVariants(int threshold)
        {
            return _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Stock <= threshold)
                .Select(v => new LowStockVariantDto
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    Size = v.Size,
                    Stock = v.Stock
                }).ToList();
        }
    }
}
