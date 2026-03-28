namespace ClientEcommerce.API.Enum
{
    public enum OrderStatus
    {
        // USER
        Placed,

        // ADMIN
        Confirmed,
        Rejected,

        // FULFILLMENT
        Dispatched,
        Delivered,
        Cancelled
    }
}
