using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.DTOs.Admin;

namespace ClientEcommerce.API.Services
{
    public interface IOrderService
    {
        // ===========================
        // CUSTOMER
        // ===========================
        Task PlaceOrder(int userId, PlaceOrderByCustomerDto dto);
        IEnumerable<UserOrderListDto> GetOrdersForUser(int userId);
        UserOrderDetailDto GetOrderForUser(int orderId, int userId);
        Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId);
        void CancelCustomerOrder(int userId, int orderId, string reason);
        Task<PagedResultDto<AdminOrderListDto>> GetAdminOrders(int page, int pageSize, string? status);




        // ===========================
        // ADMIN
        // ===========================
        void ConfirmOrder(int orderId);
        
        void CancelOrder(int orderId, string reason);
        void DispatchOrder(int orderId);
        void DeliverOrder(int orderId);

        Task<AdminOrderDetailDto?> GetOrderByIdAsync(int orderId);
        IEnumerable<AdminOrderListDto> GetRecentOrders(int count);
    }
}
