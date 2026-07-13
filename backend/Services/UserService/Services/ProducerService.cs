using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using UserService.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace UserService.Services
{
    public class ProducerService : IProducer
    {
        private readonly ILogger<ProducerService> _logger;
        private readonly RabbitMQConnection _connection;

        public ProducerService(ILogger<ProducerService> logger, RabbitMQConnection connection)
        {
            this._logger = logger;
            this._connection = connection;
         
        }

        // Méthode à utiliser pour consommer tous les events que le service user-service reçoit
        public void SendEvent<T>(T message, string queueName)
        {
            if (message == null) { return; }

            try
            {
                // Déclare une file d'attente pour garantir qu'elle existe avant d'envoyer des messages
                this._connection.GetChannel().QueueDeclareAsync(queue: queueName,
                                     durable: false,  // Ne persiste pas les messages
                                     exclusive: false, // La file d'attente peut être partagée
                                     autoDelete: false, // La file d'attente ne se supprime pas automatiquement
                                     arguments: null);

                // Convertir le message en tableau d'octets
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                // Publier le message dans la file d'attente
                this._connection.GetChannel().BasicPublishAsync(exchange: "",
                                     routingKey: queueName, // Nom de la file d'attente
                                     body: body);

                this._logger.LogInformation($"Event send : {message}");
            }
            catch (Exception ex)
            {
                this._logger.LogError($"An error occured while publishing event: {ex.Message}");
            }
        }
    }
}
