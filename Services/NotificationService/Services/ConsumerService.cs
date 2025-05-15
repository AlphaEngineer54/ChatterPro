using Newtonsoft.Json;
using NotificationService.Events;
using NotificationService.Interfaces;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService.Services
{
    public class ConsumerService : IConsumer, IDisposable
    {
        private readonly NotificationManagerService _notificationService;

        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(NotificationManagerService notificationService, ILogger<ConsumerService> logger)
        {
            _notificationService = notificationService;
            var connection = new ConnectionFactory()
            {
                // Set up properties for RabbitMQ connection
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connection.CreateConnection();
            _channel = _connection.CreateModel();
            _logger = logger;

            _logger.LogInformation("Connection to RabbitMQ server initialized successfully!");
        }

        public Task ConsumeEvent(string queueName)
        {
            // Consume event from the queue
            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);


            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();

                try
                {
                    var message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(body), typeof(CreatedMessageEvent)) as CreatedMessageEvent;

                    if (message == null)
                    {
                        _logger.LogWarning("Received null message");
                        return;
                    }

                    var notification = new Notification()
                    {
                        UserId = message.ReceiverId,
                        Message = $"user-{message.SenderId}:{message.Message}",
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _notificationService.AddNotification(notification);

                    _logger.LogInformation($"Received message: {message}");

                    // Manually acknowledge the message after successful processing
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while consuming events: {ex.Message}");
                    // If an error occurs, negatively acknowledge the message so it can be retried
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

            _logger.LogInformation($"Consumer is listening on queue: {queueName}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.LogInformation("Connection to RabbitMQ server closed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to close RabbitMQ connection: {ex.Message}");
            }
        }
    }
}
