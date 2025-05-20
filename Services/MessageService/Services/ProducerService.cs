using MessageService.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace MessageService.Services
{
    public class ProducerService : IProducer
    {
        private readonly RabbitMQConnection _connection;
        private readonly ILogger<ProducerService> _logger;  

        public ProducerService(RabbitMQConnection connection, ILogger<ProducerService> logger)
        {
            this._logger = logger;
            this._connection = connection;
            
            this._logger.LogInformation("ProducerService has been initialized!");
        }

        void IProducer.Send<T>(T message, string queueName)
        {
            try
            {
                this._connection.GetChannel()
                                .QueueDeclare(queueName, 
                                           durable: false,
                                           exclusive: false, 
                                           autoDelete: false);

                // Convertir le message en tableau d'octets
                var eventMessage = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(eventMessage);

                // Publier le message dans la file d'attente
                _connection.GetChannel().BasicPublish(exchange: "",
                                     routingKey: queueName, // Nom de la file d'attente
                                     body: body);

                this._logger.LogInformation($"Event send : {eventMessage}");
            } 
            catch(Exception ex)
            {
                this._logger.LogError($"An error occured while publishing event : {ex.Message}");
            }
        }
    }
}
