using OrderTrackingSystem.DTOs;

namespace OrderTrackingSystem.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request);
        Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(string orderId);
        Task<ApiResponse<List<OrderResponse>>> GetAllOrdersAsync();
        Task<ApiResponse<List<OrderResponse>>> GetOrdersByUserIdAsync(int userId);
        Task<ApiResponse<List<OrderResponse>>> GetOrdersByStatusAsync(string status);
    }
}
