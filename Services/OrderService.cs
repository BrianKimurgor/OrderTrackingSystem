using OrderTrackingSystem.Data;
using OrderTrackingSystem.DTOs;
using OrderTrackingSystem.Models;

namespace OrderTrackingSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository repository,
            IKafkaProducerService kafkaProducer,
            ILogger<OrderService> logger)
        {
            _repository = repository;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                var order = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    ProductName = request.ProductName,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    Status = "PLACED",
                    CreatedAt = DateTime.UtcNow
                };

                await _kafkaProducer.PublishOrderAsync(order);

                var response = MapToResponse(order);

                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order created and published successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Failed to create order: " + ex.Message
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(string orderId)
        {
            var order = await _repository.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            return new ApiResponse<OrderResponse>
            {
                Success = true,
                Message = "Order retrieved successfully",
                Data = MapToResponse(order)
            };
        }

        public async Task<ApiResponse<List<OrderResponse>>> GetAllOrdersAsync()
        {
            var orders = await _repository.GetAllOrdersAsync();

            return new ApiResponse<List<OrderResponse>>
            {
                Success = true,
                Message = $"Retrieved {orders.Count} orders",
                Data = orders.Select(MapToResponse).ToList()
            };
        }

        public async Task<ApiResponse<List<OrderResponse>>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _repository.GetOrdersByUserIdAsync(userId);

            return new ApiResponse<List<OrderResponse>>
            {
                Success = true,
                Message = $"Retrieved {orders.Count} orders for user {userId}",
                Data = orders.Select(MapToResponse).ToList()
            };
        }

        public async Task<ApiResponse<List<OrderResponse>>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _repository.GetOrdersByStatusAsync(status);

            return new ApiResponse<List<OrderResponse>>
            {
                Success = true,
                Message = $"Retrieved {orders.Count} orders with status '{status}'",
                Data = orders.Select(MapToResponse).ToList()
            };
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                Price = order.Price,
                TotalAmount = order.Price * order.Quantity,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ProcessedAt = order.ProcessedAt
            };
        }
    }
}
