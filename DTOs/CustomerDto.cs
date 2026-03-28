namespace ClientEcommerce.API.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string CompanyName { get; set; } = "";
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
