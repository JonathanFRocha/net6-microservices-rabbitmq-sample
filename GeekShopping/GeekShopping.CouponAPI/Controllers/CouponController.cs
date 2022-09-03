using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CouponAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _repository;
        public CouponController(ICouponRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("{couponName}")]
        public async Task<ActionResult<CouponVO>> FindByCouponName(string couponName)
        {
            var coupon = await _repository.GetCouponByCouponCode(couponName);
            if (coupon == null) return NotFound();
            return Ok(coupon);
        }
    }
}