using GeekShopping.Email.Messages;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.Email.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly EmailRepository _emailRepository;
        private IConnection _connection;
        private IModel _channel;
        private const string _exchangeName = "DirectPaymentUpdateExchange";
        private const string _paymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";
        

        public RabbitMQPaymentConsumer(EmailRepository emailRepository, IConfiguration configuration)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["Messanger:Hostname"],
                Password = configuration["Messanger:Password"],
                UserName = configuration["Messanger:Username"],
            };

            _emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(_paymentEmailUpdateQueueName, false, false, false, null);

            _channel.QueueBind(_paymentEmailUpdateQueueName,_exchangeName, "PaymentEmail");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                var message = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(content);
                await ProcessLogs(message);
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume(_paymentEmailUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessLogs(UpdatePaymentResultMessage message)
        {
            try
            {
                await _emailRepository.LogEmail(message);
            }
            catch (Exception)
            {
                // Log
                throw;
            }
        }
    }
}
