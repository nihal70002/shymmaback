using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.DTOs.Admin;

namespace ClientEcommerce.API.Services
{
    public interface IOrderService
    {
        // ===========================
        // CUSTOMER
        // ===========================
        void PlaceOrder(int userId, PlaceOrderByCustomerDto dto);
        IEnumerable<UserOrderListDto> GetOrdersForUser(int userId);
        UserOrderDetailDto GetOrderForUser(int orderId, int userId);
        Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId);
        Task<PagedResultDto<AdminOrderListDto>> GetAdminOrders(int page, int pageSize, string? status);




        // ===========================
        // ADMIN
        // ===========================
        void ConfirmOrder(int orderId);
        
        void CancelOrder(int orderId, string reason);
        void DispatchOrder(int orderId);
        void DeliverOrder(int orderId);

        AdminOrderDetailDto GetOrderById(int orderId);
        IEnumerable<AdminOrderListDto> GetRecentOrders(int count);
    }
}
