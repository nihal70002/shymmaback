using System.Text.Json.Serialization;

namespace ClientEcommerce.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        // =======================
        // USER
        // =======================
        public int UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; } = null!;

        // =======================
        // ORDER STATUS
        // =======================
        // Pending | Confirmed | Dispatched | Delivered | Cancelled
        public string Status { get; set; } = "Pending";

        public string? CancelReason { get; set; }

        public DateTime? AdminApprovedAt { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // =======================
        // ORDER INFO
        // =======================
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
