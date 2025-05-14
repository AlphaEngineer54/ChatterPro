namespace AuthService.Interfaces
{
    public interface IConsumer
    {
        public Task ConsumeEvent(string queueName);
    }
}
