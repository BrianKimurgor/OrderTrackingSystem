using OrderTrackingSystem.Models;

namespace OrderTrackingSystem.Data
{
    public interface IOrderRepository
    {
        Task SaveOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(string orderId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<List<Order>> GetOrdersByStatusAsync(string status);
    }
}
