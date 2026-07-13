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


                this._connection = factory.CreateConnectionAsync().Result;
                this._channel = _connection.CreateChannelAsync().Result;

                this._logger.LogInformation($"Connection to RabbitMQ server initialized successfully!");
            }
           catch (Exception ex)
           {
              this._logger.LogError($"Failed to connect to RabbitMQ Server Error message: {ex.Message} ");
           }
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
