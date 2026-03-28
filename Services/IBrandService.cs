using ClientEcommerce.API.DTOs;

public interface IBrandService
{
    List<BrandListDto> GetBrands();
    void CreateBrand(CreateBrandDto dto);
}
