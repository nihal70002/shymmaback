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
        public string? ProductNameSnapshot { get; set; }
        public string? SizeSnapshot { get; set; }
        public string? StyleSnapshot { get; set; }
        public string? MaterialSnapshot { get; set; }
        public string? ColorSnapshot { get; set; }
        public string? ClassSnapshot { get; set; }
        public string? ProductCodeSnapshot { get; set; }
    }
}
