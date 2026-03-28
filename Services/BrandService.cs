using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;


public class BrandService : IBrandService
{
    private readonly AppDbContext _context;

    public BrandService(AppDbContext context)
    {
        _context = context;
    }


    public List<BrandListDto> GetBrands()
    {
        return _context.Brands
            .OrderBy(b => b.BrandName)
            .Select(b => new BrandListDto
            {
                BrandId = b.BrandId,
                BrandName = b.BrandName
            })
            .ToList();
    }

    public void CreateBrand(CreateBrandDto dto)
    {
        if (_context.Brands.Any(b => b.BrandName == dto.BrandName))
            throw new Exception("Brand already exists");

        _context.Brands.Add(new Brand
        {
            BrandName = dto.BrandName
        });

        _context.SaveChanges();
    }
}
