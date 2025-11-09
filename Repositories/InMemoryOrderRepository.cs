using OrderTrackingSystem.Models;


namespace OrderTrackingSystem.Data
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<string, Order> _orders = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly ILogger<InMemoryOrderRepository> _logger;

        public InMemoryOrderRepository(ILogger<InMemoryOrderRepository> logger)
        {
            _logger = logger;
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _lock.WaitAsync();
            try
            {
                _orders[order.OrderId] = order;
                _logger.LogInformation("💾 Saved order {OrderId} to repository", order.OrderId);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            await _lock.WaitAsync();
            try
            {
                return _orders.TryGetValue(orderId, out var order) ? order : null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return _orders.Values
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            await _lock.WaitAsync();
            try
            {
                return _orders.Values
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            await _lock.WaitAsync();
            try
            {
                return _orders.Values
                    .Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
