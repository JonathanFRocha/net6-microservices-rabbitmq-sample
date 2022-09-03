using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender.Interfaces;
using GeekShopping.CartAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRespository;
        private readonly ICouponRepository _couponRespository;
        private readonly IRabbitMQMessageSender _rabbitMQMessageSender;
        public CartController(ICartRepository repository, IRabbitMQMessageSender rabbitMQMessageSender, ICouponRepository couponRespository)
        {
            _cartRespository = repository ?? throw new ArgumentNullException(nameof(repository));
            _rabbitMQMessageSender = rabbitMQMessageSender ?? throw new ArgumentNullException(nameof(rabbitMQMessageSender));
            _couponRespository = couponRespository ?? throw new ArgumentNullException(nameof(couponRespository));
        }

        [HttpGet("find-cart/{userId}")]
        public async Task<ActionResult<CartVO>> FindById(string userId)
        {
            var cart = await _cartRespository.FindCartByUserId(userId);
            if (cart == null) return NotFound();
            return Ok(cart);
        }


        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart(CartVO vo)
        {
            var cart = await _cartRespository.SaveOrUpdateCart(vo);
            if (cart == null) return NotFound();
            return Ok(cart);
        }


        [HttpPost("apply-coupon")]
        public async Task<ActionResult<bool>> ApplyCoupon(CartVO vo)
        {
            var status = await _cartRespository.ApplyCoupon(vo.CartHeader.UserId, vo.CartHeader.CouponCode);
            if (!status) return NotFound();
            return Ok(status);
        }

        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
        {
            var status = await _cartRespository.RemoveCoupon(userId);
            if (!status) return NotFound();
            return Ok(status);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO vo)
        {
            if (vo?.UserId == null) return BadRequest();
            var cart = await _cartRespository.FindCartByUserId(vo.UserId);
            if (cart == null) return NotFound();

            if(!string.IsNullOrEmpty(vo.CouponCode))
            {
                var token = Request.Headers["Authorization"];
                var coupon = await _couponRespository.GetCouponByCouponCode(vo.CouponCode, token);
                if(vo.DiscountTotal != coupon.DiscountAmount)
                {
                    return StatusCode(412);
                }
            }

            vo.CartDetails = cart.CartDetails;
            vo.DateTime = DateTime.Now;

            //call rabbitmq
            _rabbitMQMessageSender.SendMessage(vo, "CheckoutQueue");

            await _cartRespository.ClearCart(vo.UserId);

            return Ok(vo);
        }

        [HttpPut("update-cart")]
        public async Task<ActionResult<CartVO>> UpdateCart(CartVO vo)
        {
            var cart = await _cartRespository.SaveOrUpdateCart(vo);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<bool>> RemoveCart(int id)
        {
            var status = await _cartRespository.RemoveFromCart(id);
            if (!status) return BadRequest();
            return Ok(status);
        }
    }
}