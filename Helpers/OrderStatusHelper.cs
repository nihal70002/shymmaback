using ClientEcommerce.API.Enum;

namespace ClientEcommerce.API.Helpers
{
    public static class OrderStatusHelper
    {
        public static string GetCustomerStatus(string internalStatus)
        {
            return internalStatus switch
            {
                nameof(OrderStatus.Placed) => "Pending",
                nameof(OrderStatus.Confirmed) => "Confirmed",
                nameof(OrderStatus.Rejected) => "Cancelled",

                nameof(OrderStatus.Dispatched) => "Dispatched",
                nameof(OrderStatus.Delivered) => "Delivered",
                nameof(OrderStatus.Cancelled) => "Cancelled",

                _ => "Pending"
            };
        }
    }
}
