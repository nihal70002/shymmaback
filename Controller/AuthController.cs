using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.DTOs.Auth;
using ClientEcommerce.API.Models;
using ClientEcommerce.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClientEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly IPasswordHasher<User> _passwordHasher;



        public AuthController(
     AppDbContext context,
     IConfiguration config,
     IAuthService authService,
     IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _config = config;
            _authService = authService;
            _passwordHasher = passwordHasher;
        }


        // 🔐 LOGIN
        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.LoginId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Invalid login credentials");
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email == request.LoginId ||
                u.PhoneNumber == request.LoginId);

            if (user == null)
                return Unauthorized("Invalid login credentials");

            // 🚫 BLOCK INACTIVE USERS
            if (!user.IsActive)
            {
                return Unauthorized(
                    "Your account is inactive. Please contact admin."
                );
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );


            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid login credentials");

            // ✅ Optional: auto-upgrade hash
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(user, request.Password);
                _context.SaveChanges();
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token,
                user.Id,
                user.Name,
                user.Role
            });
        }
       



        // 🔑 CHANGE PASSWORD (Logged-in)
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("Invalid password data");
            }

            await _authService.ChangePasswordAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                dto.CurrentPassword,
                dto.NewPassword
            );

            return Ok(new { message = "Password changed successfully" });
        }

        // 📧 FORGOT PASSWORD (SAFE & IDEMPOTENT)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
      [FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            await _authService.ForgotPasswordAsync(dto.Email);

            return Ok("If email exists, reset link sent");
        }



        // 🔄 RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("Invalid reset data");
            }

            await _authService.ResetPasswordAsync(
                dto.Token,
                dto.NewPassword);

            return Ok("Password reset successful");
        }
      


        // 🔐 JWT TOKEN GENERATION
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_config["Jwt:DurationInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
