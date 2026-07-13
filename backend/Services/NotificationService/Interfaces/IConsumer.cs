namespace NotificationService.Interfaces
{
    public interface IConsumer
    {
        Task ConsumeEvent(string queueName);
    }
}
