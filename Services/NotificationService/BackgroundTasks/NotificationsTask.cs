using NotificationService.Interfaces;

namespace NotificationService.BackgroundTasks
{
    /// <summary>
    /// NotificationsTask is a background service that consumes events from a message queue.
    /// </summary>
    public class NotificationsTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationsTask> _logger;

        public NotificationsTask(IServiceProvider serviceProvider, ILogger<NotificationsTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetRequiredService<IConsumer>();

                    try
                    {
                        // Start consuming events from the message queue (the method itself should be non-blocking)
                        await consumer.ConsumeEvent("new-message-event");
                    }
                    catch (Exception ex)
                    {
                        // Log the error, depending on your logging strategy
                        _logger.LogError($"Error in NotificationsTask: {ex.Message}");
                    }
                }

                // Wait briefly before restarting the loop, if needed
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
