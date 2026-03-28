public interface IAdminReportService
{
    SalesSummaryDto GetSalesSummary();
    IEnumerable<SalesTrendDto> GetMonthlySales(int year);
    IEnumerable<TopProductDto> GetTopProducts(int top = 5);
    IEnumerable<TopCustomerDto> GetTopCustomers(int top = 5);
    IEnumerable<CustomerProductInterestDto> GetCustomerProductInterest(int userId);

}
