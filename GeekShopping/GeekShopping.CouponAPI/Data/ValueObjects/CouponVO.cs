namespace GeekShopping.CouponAPI.Data.ValueObjects
{
    public class CouponVO
    {
        public long Id { get; set; }

        public string CouponCode { get; set; } = string.Empty;

        public decimal DiscountAmount { get; set; }

    }
}
