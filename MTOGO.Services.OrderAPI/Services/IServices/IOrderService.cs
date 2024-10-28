using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;

namespace MTOGO.Services.OrderAPI.Services.IServices
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(OrderDto order);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<int> UpdateOrderStatusAsync(int orderId, int statusId);
        Task<int> DeleteOrderAsync(int id);
    }
}
