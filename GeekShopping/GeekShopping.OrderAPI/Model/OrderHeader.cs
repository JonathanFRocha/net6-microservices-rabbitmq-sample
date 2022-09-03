using GeekShopping.OrderAPI.Model.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekShopping.OrderAPI.Model
{
    [Table("order_header")]
    public class OrderHeader:BaseEntity
    {

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("coupon_code")]
        public string CouponCode { get; set; } = string.Empty;
        [Column("purchase_amount")]
        public decimal PurchaseAmount { get; set; }
        [Column("discount_total")]
        public decimal DiscountTotal { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;
        [Column("purchase_date")]
        public DateTime DateTime { get; set; }
        [Column("order_time")]
        public DateTime OrderTime { get; set; }
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        [Column("card_number")]
        public string CardNumber { get; set; } = string.Empty;
        [Column("cvv")]
        public string CVV { get; set; } = string.Empty;
        [Column("expiry_month_year")]
        public string ExpiryMonthYear { get; set; } = string.Empty;
        [Column("order_total_items")]
        public int CartTotalItems { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
        [Column("payment_status")]
        public bool PaymentStatus { get; set; }
    }
}
