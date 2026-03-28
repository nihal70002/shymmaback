namespace ClientEcommerce.API.DTOs
{
    public class UpdateUserProfileDto
    {
        public required string Name { get; set; }
        public required string PhoneNumber { get; set; }
    }
}