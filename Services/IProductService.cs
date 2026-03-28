using ClientEcommerce.API.DTOs;

namespace ClientEcommerce.API.Services
{
    public interface IProductService
    {
        IEnumerable<ProductListDto> GetAllProducts();

        PagedResponseDto<ProductListDto> GetProducts(
            int page,
            int pageSize,
            List<int>? categoryIds,
            int? brandId,
            string? search
        );

        ProductDetailDto GetProductById(int productId);

        void CreateProduct(AdminCreateProductDto dto);

        void BulkCreate(List<AdminBulkCreateProductDto> products);

        void UpdateProduct(int productId, AdminUpdateProductDto dto);

        void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto);

        void UpdateVariantStock(int variantId, int stock);

        void AddProductVariant(int productId, AdminCreateProductVariantDto dto);

        void ToggleProduct(int productId);

        Task DeleteProductAsync(int productId);

        IEnumerable<LowStockVariantDto> GetLowStockVariants(int threshold);
    }
}