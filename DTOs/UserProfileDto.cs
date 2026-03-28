namespace ClientEcommerce.API.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        // Using ? because not every customer belongs to a company
        public string? CompanyName { get; set; }   // 👈 ADDED

        public required string Email { get; set; }

        public required string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}