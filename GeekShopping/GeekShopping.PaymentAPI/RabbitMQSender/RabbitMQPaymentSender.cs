using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender.Interfaces;
using GeekShopping.MessageBus;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.RabbitMQSender
{
    public class RabbitMQPaymentSender : IRabbitMQPaymentSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        private IConnection _connection;
        private const string _exchangeName = "DirectPaymentUpdateExchange";
        private const string _paymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";
        private const string _paymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";

        public RabbitMQPaymentSender(IConfiguration configuration)
        {
            _hostName = configuration["Messanger:Hostname"];
            _password= configuration["Messanger:Password"];
            _userName = configuration["Messanger:Username"];
        }
        public void SendMessage(BaseMessage message)
        {
            try
            {
                if (ConnectionExists())
                {
                    using var channel = _connection.CreateModel();
                    channel.ExchangeDeclare(exchange: _exchangeName, ExchangeType.Direct, durable: false);

                    channel.QueueDeclare(_paymentOrderUpdateQueueName, false, false, false, null);
                    channel.QueueDeclare(_paymentEmailUpdateQueueName, false, false, false, null);

                    channel.QueueBind(_paymentOrderUpdateQueueName, _exchangeName, "PaymentOrder");
                    channel.QueueBind(_paymentEmailUpdateQueueName, _exchangeName, "PaymentEmail");

                    byte[] body = GetMessageAsByteArray(message);
                    channel.BasicPublish(exchange: _exchangeName, "PaymentOrder", basicProperties: null, body: body);
                    channel.BasicPublish(exchange: _exchangeName, "PaymentEmail", basicProperties: null, body: body);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException();
            }

        }

        private byte[] GetMessageAsByteArray(BaseMessage message)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, };
            var json = JsonSerializer.Serialize((UpdatePaymentResultMessage)message, options);
            return Encoding.UTF8.GetBytes(json);
        }

        private bool ConnectionExists()
        {
            if (_connection != null) return true;
            CreateConnection();
            return _connection != null;
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _userName,
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception e)
            {
                // log exception
                throw;
            }
        }
    }
}
