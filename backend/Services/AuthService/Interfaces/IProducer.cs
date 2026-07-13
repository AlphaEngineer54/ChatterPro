namespace AuthService.Interfaces
{
    public interface IProducer
    {
        public void PublishEvent<T>(T message, string queueName);
    }
}
