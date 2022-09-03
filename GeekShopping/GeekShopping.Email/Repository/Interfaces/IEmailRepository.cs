using GeekShopping.Email.Messages;

namespace GeekShopping.Email.Repository.Interfaces
{
    public interface IEmailRepository
    {
        Task LogEmail(UpdatePaymentResultMessage message);

    }
}
