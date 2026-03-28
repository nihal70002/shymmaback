namespace ClientEcommerce.API.DTOs
{
    public class CreateAddressDto
    {
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public required string AddressLine { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Pincode { get; set; }
        public bool IsDefault { get; set; }
    }
}