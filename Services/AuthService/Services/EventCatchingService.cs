
using AuthService.Interfaces;

namespace AuthService.Services
{
    public class EventCatchingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public EventCatchingService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetService<IConsumer>();

                await consumer.ConsumeEvent("user-updated");
                await consumer.ConsumeEvent("user-deleted");

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
