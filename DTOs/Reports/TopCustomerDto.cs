public class TopCustomerDto
{
    public int UserId { get; set; }
    public required string CustomerName { get; set; }
    public required string Email { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
}
