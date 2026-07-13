using NotificationService.Interfaces;

namespace NotificationService.BackgroundTasks
{
    /// <summary>
    /// NotificationsTask is a background service that consumes events from a message queue.
    /// </summary>
    public class NotificationsTask : BackgroundService
    {
        private readonly IConsumer _consumer;
        private readonly ILogger<NotificationsTask> _logger;

        public NotificationsTask(IConsumer consumer, ILogger<NotificationsTask> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consumer listening RabbitMQ events...");

            try
            {
                await _consumer.ConsumeEvent("new-message-event");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in NotificationsTask: {ex.Message}");
            }
        }
    }
}


