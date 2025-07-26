using UserService.Interfaces;

namespace UserService.Services
{
    public class ConsumerBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<ConsumerBackgroundService> _logger;

        public ConsumerBackgroundService(IServiceProvider provider, ILogger<ConsumerBackgroundService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Listening RabbitMQ events...");

            // Create a scope to resolve scoped services like IConsumer
            using var scope = _provider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IConsumer>();

            // Démarrer la consommation des événements
            consumer.ConsumeEvent("user-created");
            consumer.ConsumeEvent("get-user-event");

            return Task.CompletedTask; // Garde le service actif
        }
    }
}
