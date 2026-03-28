using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;
using System.Security.Claims;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    [HttpGet]
    public IActionResult GetAddresses()
    {
        return Ok(_addressService.GetAddresses(UserId));
    }

    [HttpPost]
    public IActionResult AddAddress(CreateAddressDto dto)
    {
        _addressService.AddAddress(UserId, dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAddress(int id)
    {
        _addressService.DeleteAddress(UserId, id);
        return Ok();
    }

    [HttpPut("{id}/default")]
    public IActionResult SetDefault(int id)
    {
        _addressService.SetDefaultAddress(UserId, id);
        return Ok();
    }
}
