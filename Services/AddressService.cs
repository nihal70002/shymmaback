using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;


public class AddressService : IAddressService
{
    private readonly AppDbContext _context;

    public AddressService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<AddressDto> GetAddresses(int userId)
    {
        return _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Phone = a.Phone,
                AddressLine = a.AddressLine,
                City = a.City,
                State = a.State,
                Pincode = a.Pincode,
                IsDefault = a.IsDefault
            })
            .ToList();
    }

    public void AddAddress(int userId, CreateAddressDto dto)
    {
        if (dto.IsDefault)
        {
            var existingDefaults = _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault);

            foreach (var addr in existingDefaults)
                addr.IsDefault = false;
        }

        var address = new Address
        {
            UserId = userId,
            FullName = dto.FullName,
            Phone = dto.Phone,
            AddressLine = dto.AddressLine,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            IsDefault = dto.IsDefault
        };

        _context.Addresses.Add(address);
        _context.SaveChanges();
    }

    public void DeleteAddress(int userId, int addressId)
    {
        var address = _context.Addresses
            .FirstOrDefault(a => a.Id == addressId && a.UserId == userId);

        if (address == null) return;

        _context.Addresses.Remove(address);
        _context.SaveChanges();
    }

    public void SetDefaultAddress(int userId, int addressId)
    {
        var addresses = _context.Addresses.Where(a => a.UserId == userId);

        foreach (var addr in addresses)
            addr.IsDefault = addr.Id == addressId;

        _context.SaveChanges();
    }
}
