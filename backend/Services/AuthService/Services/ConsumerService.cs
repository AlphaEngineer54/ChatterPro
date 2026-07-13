using AuthService.Events;
using AuthService.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AuthService.Services
{

    public class ConsumerService : IConsumer
    {
        private readonly ILogger<ConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQConnection _connection;

        public ConsumerService(ILogger<ConsumerService> logger, RabbitMQConnection connection, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._connection = connection;
        }

        public async Task ConsumeEvent(string queueName)
        {
            // Déclare la file d'attente
            await this._connection.GetChannel().QueueDeclareAsync(queue: queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new AsyncEventingBasicConsumer(this._connection.GetChannel());
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();

                try
                {
                    // Crée une portée pour résoudre IEventHandler
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IEventHandler>();

                    var messageType = queueName switch
                    {
                        "user-updated" => typeof(UserUpdatedEvent),
                        "user-deleted" => typeof(UserDeletedEvent),
                        _ => throw new ArgumentException($"Unhandled queue name: {queueName}")
                    };

                    var message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(body), messageType) as Event;
                    await handler.HandleEventAsync(message ?? throw new ArgumentException("Event is null!"));

                    _logger.LogInformation($"Received message: {message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while consuming events: {ex.Message}");
                }
            };

            await this._connection.GetChannel().BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
