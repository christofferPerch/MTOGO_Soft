using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace MTOGO.MessageBus
{
    public class MessageBus : IMessageBus, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private bool _disposed = false;

        public MessageBus(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        }

        public async Task PublishMessage(string queueName, string message)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName), "Queue name cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message payload cannot be null or empty.");
            }

            try
            {

                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish message to {queueName}: {ex.Message}", ex);
            }
        }



        public void SubscribeMessage<T>(string queueName, Action<T> onMessageReceived)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName), "Queue name cannot be null or empty.");
            }

            var channel = _connection.CreateModel();

            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var data = JsonConvert.DeserializeObject<T>(message);
                    onMessageReceived?.Invoke(data);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);

                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }

            _disposed = true;
        }

    }
}
