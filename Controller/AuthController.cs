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
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;

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
        [EnableRateLimiting("LoginPolicy")] // ✅ Rate limiting added
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
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            SetRefreshTokenCookie(refreshToken);

            return Ok(new
            {
                message = "Login successful",
                token,
                user.Id,
                user.Name,
                user.Role
            });
        }
       
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token provided.");

            var user = _context.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            SetRefreshTokenCookie(newRefreshToken);

            return Ok(new { token = newJwtToken });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
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
                    Convert.ToDouble(_config["Jwt:ExpiresInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
