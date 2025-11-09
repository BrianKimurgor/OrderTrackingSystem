using System.ComponentModel.DataAnnotations;

namespace OrderTrackingSystem.DTOs
{
    public class CreateOrderRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }
}
