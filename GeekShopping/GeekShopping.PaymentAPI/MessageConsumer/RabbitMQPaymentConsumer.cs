using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender.Interfaces;
using GeekShopping.PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQPaymentSender _rabbitMQPaymentSender;
        private readonly IProcessPayment _processPayment;


        public RabbitMQPaymentConsumer(IConfiguration configuration, IProcessPayment processPayment, IRabbitMQPaymentSender rabbitMQPaymentSender)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["Messanger:Hostname"],
                Password = configuration["Messanger:Password"],
                UserName = configuration["Messanger:Username"],
            };

            _processPayment = processPayment ?? throw new ArgumentNullException(nameof(processPayment));
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "orderpaymentprocessqueue", false, false, false, arguments: null);
            _rabbitMQPaymentSender = rabbitMQPaymentSender ?? throw new ArgumentNullException(nameof(rabbitMQPaymentSender));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                var vo = JsonSerializer.Deserialize<PaymentMessage>(content);
                await ProcessPayment(vo);
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("orderpaymentprocessqueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessPayment(PaymentMessage vo)
        {
            var result = _processPayment.PaymentProcessor();
            var paymentResult = new UpdatePaymentResultMessage()
            {
                Status = result,
                OrderId = vo.OrderId,
                Email = vo.Email,
            };
            try
            {
                _rabbitMQPaymentSender.SendMessage(paymentResult);
            }
            catch (Exception)
            {
                // Log
                throw;
            }
        }
    }
}
