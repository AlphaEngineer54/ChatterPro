using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using UserService.Interfaces;

namespace UserService.Services
{
    public class ConsumerBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;

        public ConsumerBackgroundService(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
