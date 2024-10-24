using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MTOGO.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly IConfiguration _configuration;

        public MessageBus(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishMessage(string queueName, string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare a queue if it doesn't exist already
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            // Publish the message
            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

            await Task.CompletedTask;
        }
    }
}
