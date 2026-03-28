using ClientEcommerce.API.DTOs;
using System.Collections.Generic;

public interface IAddressService
{
    IEnumerable<AddressDto> GetAddresses(int userId);
    void AddAddress(int userId, CreateAddressDto dto);
    void DeleteAddress(int userId, int addressId);
    void SetDefaultAddress(int userId, int addressId);
}
