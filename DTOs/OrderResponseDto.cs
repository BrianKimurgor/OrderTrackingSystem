namespace OrderTrackingSystem.DTOs
{
    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
