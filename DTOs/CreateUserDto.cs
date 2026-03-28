namespace ClientEcommerce.API.DTOs
{
    public class CreateUserDto
    {
        public required string Name { get; set; }

        // Use ? if company name is optional, otherwise use required
        public string? CompanyName { get; set; }

        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
    }
}