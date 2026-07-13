namespace UserService.Interfaces
{
    public interface IConsumer
    {
         void ConsumeEvent(string queueName);
    }
}
