using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Services;
using System.Security.Claims;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private int UserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // =========================
        // USER PROFILE
        // =========================
        [HttpGet("me")]
        public IActionResult GetProfile()
        {
            var profile = _userService.GetProfile(UserId);
            return Ok(profile);
        }

        // =========================
        // UPDATE PROFILE
        // =========================
        [HttpPut("me")]
        public IActionResult UpdateProfile(UpdateUserProfileDto dto)
        {
            _userService.UpdateProfile(UserId, dto);
            return Ok("Profile updated");
        }

        // =========================
        // CHANGE PASSWORD
        // =========================
        [HttpPut("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto dto)
        {
            _userService.ChangePassword(UserId, dto);
            return Ok("Password changed successfully");
        }
    }
}
