using System.Text.Json.Serialization;

namespace ClientEcommerce.API.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        public Order Order { get; set; }

        // ✅ ONLY ProductVariant
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
