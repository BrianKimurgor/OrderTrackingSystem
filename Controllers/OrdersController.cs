using Microsoft.AspNetCore.Mvc;
using OrderTrackingSystem.DTOs;
using OrderTrackingSystem.Services;


namespace OrderTrackingSystem.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new order and publishes it to Kafka
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            _logger.LogInformation("Creating order for user {UserId}", request.UserId);

            var result = await _orderService.CreateOrderAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all orders
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<OrderResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific order by ID
        /// </summary>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var result = await _orderService.GetOrderByIdAsync(orderId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all orders for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<List<OrderResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            var result = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all orders with a specific status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<List<OrderResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            var result = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(result);
        }
    }
}
