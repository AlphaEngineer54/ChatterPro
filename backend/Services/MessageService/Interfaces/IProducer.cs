namespace MessageService.Interfaces
{
    /// <summary>
    /// Sent event to a message queue.
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// Produces a message of type T to the specified queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        void Send<T>(T message, string queueName);
    }
}
 