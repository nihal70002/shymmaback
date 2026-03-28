namespace ClientEcommerce.API.DTOs.Reports
{
    public class UserPurchaseInsightDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<CustomerProductInterestDto> FavoriteProducts { get; set; }
    }
}
