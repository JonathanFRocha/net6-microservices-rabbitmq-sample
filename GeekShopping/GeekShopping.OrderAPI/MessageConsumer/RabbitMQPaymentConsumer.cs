using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.RabbitMQSender.Interfaces;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQMessageSender _rabbitMQMessageSender;
        private const string _exchangeName = "DirectPaymentUpdateExchange";
        private const string _paymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";

        public RabbitMQPaymentConsumer(OrderRepository orderRepository, IConfiguration configuration, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["Messanger:Hostname"],
                Password = configuration["Messanger:Password"],
                UserName = configuration["Messanger:Username"],
            };

            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _rabbitMQMessageSender = rabbitMQMessageSender;
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(_paymentOrderUpdateQueueName, false, false, false, null);

            _channel.QueueBind(_paymentOrderUpdateQueueName, _exchangeName, "PaymentOrder");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                var vo = JsonSerializer.Deserialize<UpdatePaymentResultVO>(content);
                await UpdatePaymentStatus(vo);
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume(_paymentOrderUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task UpdatePaymentStatus(UpdatePaymentResultVO vo)
        {
            try
            {
                await _orderRepository.UpdateOrderPaymentStatus(vo.OrderId, vo.Status);
            }
            catch (Exception)
            {
                // Log
                throw;
            }
        }
    }
}
