namespace MTOGO.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessage(string queueName, string message);

    }
}
