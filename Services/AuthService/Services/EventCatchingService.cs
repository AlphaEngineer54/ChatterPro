
using AuthService.Interfaces;

namespace AuthService.Services
{
    public class EventCatchingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventCatchingService> _logger;

        public EventCatchingService(ILogger<EventCatchingService> logger, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try
            {
                var scope = _serviceProvider.CreateScope();

                var consumer = scope.ServiceProvider.GetService<IConsumer>();

                await consumer.ConsumeEvent("user-updated");
                await consumer.ConsumeEvent("user-deleted");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                _logger.LogCritical($"An error occurred while starting the event catching service: {ex.Message}");
            }

        }
    }
}
