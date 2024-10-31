namespace MTOGO.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessage(string queueName, string message);
        void SubscribeMessage<T>(string queueName, Action<T> onMessageReceived);

    }
}
