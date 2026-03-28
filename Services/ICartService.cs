// ICartService.cs (Updated)
using ClientEcommerce.API.DTOs;
using System.Collections.Generic;

namespace ClientEcommerce.API.Services
{
    public interface ICartService
    {
        void AddToCart(int userId, AddToCartDto dto);
        IEnumerable<CartItemDto> GetCart(int userId);
        void RemoveItem(int userId, int productId);
        void UpdateQuantity(int userId, UpdateCartQuantityDto dto);
        void ClearCart(int userId);
    }
}