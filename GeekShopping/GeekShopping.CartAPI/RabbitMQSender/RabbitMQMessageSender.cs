using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender.Interfaces;
using GeekShopping.MessageBus;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.CartAPI.RabbitMQSender
{
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        private IConnection _connection;

        public RabbitMQMessageSender(IConfiguration configuration)
        {
            _hostName = configuration["Messanger:Hostname"];
            _password= configuration["Messanger:Password"];
            _userName = configuration["Messanger:Username"];
        }
        public void SendMessage(BaseMessage message, string queue)
        {
            try
            {
                if (ConnectionExists())
                {
                    using var channel = _connection.CreateModel();
                    channel.QueueDeclare(queue: queue, false, false, false, arguments: null);
                    byte[] body = GetMessageAsByteArray(message);
                    channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
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
            var json = JsonSerializer.Serialize((CheckoutHeaderVO)message, options);
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
