using System.Text.Json;
using Confluent.Kafka;
using OrderTrackingSystem.Models;


namespace OrderTrackingSystem.Services
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IConfiguration _configuration;
        private const string Topic = "orders";

        public KafkaProducerService(
            ILogger<KafkaProducerService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var config = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                ClientId = "order-producer",
                Acks = Acks.All,
                MessageTimeoutMs = 5000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _logger.LogInformation("🚀 Kafka Producer initialized");
        }

        public async Task PublishOrderAsync(Order order)
        {
            try
            {
                var json = JsonSerializer.Serialize(order);
                var message = new Message<string, string>
                {
                    Key = order.OrderId,
                    Value = json,
                    Timestamp = new Timestamp(DateTime.UtcNow)
                };

                var result = await _producer.ProduceAsync(Topic, message);

                _logger.LogInformation(
                    "✅ Published order {OrderId} to Kafka [Topic: {Topic}, Partition: {Partition}, Offset: {Offset}]",
                    order.OrderId, Topic, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "❌ Failed to publish order {OrderId} to Kafka", order.OrderId);
                throw new InvalidOperationException($"Failed to publish order {order.OrderId}", ex);
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            _logger.LogInformation("🛑 Kafka Producer disposed");
        }
    }
}
