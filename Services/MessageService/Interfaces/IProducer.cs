namespace MessageService.Interfaces
{
    public interface IProducer
    {
        void Send<T>(T message, string queueName);
    }
}
 