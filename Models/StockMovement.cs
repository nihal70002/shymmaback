namespace ClientEcommerce.API.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public int QuantityChanged { get; set; } // + / -
        public string MovementType { get; set; } = null!; // IN / OUT
        public string Reason { get; set; } = null!; // WarehouseApprove / Manual / Correction

        public int? OrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
