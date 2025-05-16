using RabbitMQ.Client;

namespace UserService.Services
{
    public class RabbitMQConnection: IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
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

                this._connection = factory.CreateConnectionAsync().Result;
                this._channel = _connection.CreateChannelAsync().Result;
           }
           catch (Exception ex)
           {
              this._logger.LogError($"Failed to connect to RabbitMQ Server Error message: {ex.Message} ");
           }

            this._logger.LogInformation($"Connection to RabbitMQ server initialized successfully!");
        }

        public IConnection GetConnection() => _connection;
        public IChannel GetChannel() => _channel;

        public void Dispose()
        {
            _connection.CloseAsync();
            _channel.CloseAsync();
        }
    }
}
