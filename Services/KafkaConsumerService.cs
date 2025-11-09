using System.Text.Json;
using Confluent.Kafka;
using OrderTrackingSystem.Data;
using OrderTrackingSystem.Models;

namespace OrderTrackingSystem.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConfiguration _configuration;
        private const string Topic = "orders";

        public KafkaConsumerService(
            IServiceProvider serviceProvider,
            ILogger<KafkaConsumerService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = _configuration["Kafka:ConsumerGroupId"] ?? "order-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                SessionTimeoutMs = 6000,
                AllowAutoCreateTopics = true
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for Kafka to be ready
            await Task.Delay(2000, stoppingToken);

            _logger.LogInformation("🎧 Kafka Consumer starting, will listen to topic '{Topic}'", Topic);

            try
            {
                _consumer.Subscribe(Topic);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ Initial subscription failed (topic may not exist yet): {Message}", ex.Message);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Use timeout to avoid blocking indefinitely
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(100));

                    if (consumeResult?.Message?.Value != null)
                    {
                        await ProcessOrderAsync(consumeResult.Message.Value, stoppingToken);
                    }
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    // Topic doesn't exist yet - this is normal on first run
                    // The topic will be auto-created when the first message is published
                    await Task.Delay(5000, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning("⚠️ Consume error: {ErrorCode} - {ErrorReason}",
                        ex.Error.Code, ex.Error.Reason);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unexpected error in consumer loop");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("🛑 Kafka Consumer shutting down gracefully");

            try
            {
                _consumer.Close();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing consumer");
            }
        }

        private async Task ProcessOrderAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                var order = JsonSerializer.Deserialize<Order>(messageValue);

                if (order != null)
                {
                    // Simulate order processing
                    order.Status = "PROCESSING";
                    order.ProcessedAt = DateTime.UtcNow;

                    await repository.SaveOrderAsync(order);

                    _logger.LogInformation(
                        "📦 Processed order {OrderId} - User: {UserId}, Product: {ProductName}, Qty: {Quantity}, Amount: ${Amount}",
                        order.OrderId, order.UserId, order.ProductName, order.Quantity, order.Price * order.Quantity);

                    // Simulate processing delay
                    await Task.Delay(200, cancellationToken);

                    // Update to completed
                    order.Status = "COMPLETED";
                    await repository.SaveOrderAsync(order);

                    _logger.LogInformation("✅ Order {OrderId} completed", order.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing order message");
            }
        }

        public override void Dispose()
        {
            try
            {
                _consumer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing consumer");
            }
            base.Dispose();
        }
    }
}