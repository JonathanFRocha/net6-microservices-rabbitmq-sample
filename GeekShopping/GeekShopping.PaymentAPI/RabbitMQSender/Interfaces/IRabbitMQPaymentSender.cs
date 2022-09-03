using GeekShopping.MessageBus;

namespace GeekShopping.PaymentAPI.RabbitMQSender.Interfaces
{
    public interface IRabbitMQPaymentSender
    {
        void SendMessage(BaseMessage baseMessage);
    }
}
