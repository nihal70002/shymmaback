using System;
using System.Collections.Generic;

namespace ClientEcommerce.API.DTOs
{
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        // 🔴 FIXED TYPE
        public List<OrderItemDetailDto> Items { get; set; } = new();
    }
}
