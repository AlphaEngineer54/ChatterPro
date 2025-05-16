using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using RabbitMQ.Client;

namespace MessageService.Services
{
    public class RabbitMQConnection : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQConnection> _logger;

        public RabbitMQConnection(ILogger<RabbitMQConnection> logger)
        {
            this._logger = logger;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "rabbitmq",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                this._connection = factory.CreateConnection();
                this._channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Failed to connect to RabbitMQ Server Error message: {ex.Message} ");
            }

            this._logger.LogInformation($"Connection to RabbitMQ server initialized successfully!");
        }

        public IConnection GetConnection() => _connection;
        public IModel GetChannel() => _channel;

        public void Dispose()
        {
            _connection.Close();
            _channel.Close();
        }
    }

}
