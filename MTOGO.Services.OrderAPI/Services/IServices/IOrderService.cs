using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;

namespace MTOGO.Services.OrderAPI.Services.IServices
{
    public interface IOrderService
    {
        Task<int> CreateOrder(OrderDto order);
        Task<OrderDto?> GetOrderById(int id);
        Task<int> UpdateOrderStatus(int orderId, int statusId);
    }
}
