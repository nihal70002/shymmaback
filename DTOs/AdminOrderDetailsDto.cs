namespace ClientEcommerce.API.DTOs
{
    public class AdminOrderDetailDto
    {
        public int OrderId { get; set; }

        // Removed 'required' and added '?' to make them nullable
        public string? CustomerName { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }

        public DateTime OrderDate { get; set; }
        public int? SalesExecutiveId { get; set; }
        public string? SalesExecutiveName { get; set; }
        public string? SalesExecutivePhone { get; set; }

        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? RejectedReason { get; set; }



        // Initialized with an empty list to prevent null reference issues
        public List<AdminOrderItemDto> Items { get; set; } = new List<AdminOrderItemDto>();
    }

    public class AdminOrderItemDto
    {
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public decimal UnitPrice { get; set; }
    }
}