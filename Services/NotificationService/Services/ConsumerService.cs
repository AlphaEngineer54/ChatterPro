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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(IServiceScopeFactory scopeFactory, ILogger<ConsumerService> logger)
        {
            _logger = logger;

            try
            {
                _scopeFactory = scopeFactory;
                var host = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? "rabbitmq";
                var portEnv = Environment.GetEnvironmentVariable("RABBIT_MQ_PORT") ?? "5672";
                var user = Environment.GetEnvironmentVariable("RABBIT_MQ_USER") ?? "guest";
                var pass = Environment.GetEnvironmentVariable("RABBIT_MQ_PASSWORD") ?? "guest";

                if (!int.TryParse(portEnv, out int port))
                {
                    throw new FormatException("La variable d’environnement 'RABBIT_MQ_PORT' doit être un entier valide.");
                }

                var factory = new ConnectionFactory
                {
                    HostName = host,
                    Port = port,
                    UserName = user,
                    Password = pass
                };

                _connection = factory.CreateConnectionAsync().Result;
                _channel = _connection.CreateChannelAsync().Result;

                _logger.LogInformation("Connection to RabbitMQ server initialized successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ Server Error message: {ex.Message} ");
            }
        }

        public async Task ConsumeEvent(string queueName)
        {
            await _channel.QueueDeclareAsync(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                _logger.LogInformation("Message received by consumer.");
                IServiceScope? scope = null;
                try
                {
                    var body = ea.Body.ToArray();
                    var raw = Encoding.UTF8.GetString(body);

                    var message = JsonConvert.DeserializeObject<CreatedMessageEvent>(raw);
                    if (message == null)
                    {
                        _logger.LogWarning("Message is null after deserialization.");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    scope = _scopeFactory.CreateScope();;

                    var notificationService = scope.ServiceProvider.GetRequiredService<NotificationManagerService>();

                    var notification = new Notification
                    {
                        UserId = message.ReceiverId,
                        Message = $"user-{message.SenderId}: {message.Message}",
                        CreatedAt = DateTime.Now,
                    };

                    await notificationService.AddNotification(notification).ConfigureAwait(false);

                    _logger.LogInformation($"Notification created: {notification.Message}");

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in consumer handler: {ex}");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                finally
                {
                    scope?.Dispose();
                }
            };

           await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }


        public void Dispose()
        {
            try
            {
                _channel?.CloseAsync();
                _connection?.CloseAsync();
                _logger.LogInformation("Connection to RabbitMQ server closed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to close RabbitMQ connection: {ex.Message}");
            }
        }
    }
}
