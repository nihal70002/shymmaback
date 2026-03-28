namespace ClientEcommerce.API.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public required string AddressLine { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Pincode { get; set; }

        public bool IsDefault { get; set; }
    }
}