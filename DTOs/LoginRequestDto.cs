namespace ClientEcommerce.API.DTOs
{
    public class LoginRequestDto
    {
        public required string LoginId { get; set; }
        public required string Password { get; set; }
    }
}