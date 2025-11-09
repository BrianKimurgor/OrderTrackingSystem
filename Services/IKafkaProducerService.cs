using OrderTrackingSystem.Models;

namespace OrderTrackingSystem.Services
{
    public interface IKafkaProducerService
    {
        Task PublishOrderAsync(Order order);
    }
}