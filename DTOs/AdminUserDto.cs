namespace ClientEcommerce.API.DTOs.Admin
{
    public class AdminUserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
    }
}
