namespace UserService.Interfaces
{
    public interface IProducer
    {
        public void SendEvent<T>(T message, string queueName);
    }
}
