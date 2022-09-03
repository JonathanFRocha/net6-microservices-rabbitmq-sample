using GeekShopping.MessageBus;

namespace GeekShopping.OrderAPI.Messages
{
    public class CheckoutHeaderVO: BaseMessage
    {
        public string UserId { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public decimal PurchaseAmount { get; set; }
        public decimal DiscountTotal { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public string ExpiryMonthYear { get; set; } = string.Empty;
        public int CartTotalItems { get; set; }
        public IEnumerable<CartDetailVO>? CartDetails { get; set; }
    }
}
