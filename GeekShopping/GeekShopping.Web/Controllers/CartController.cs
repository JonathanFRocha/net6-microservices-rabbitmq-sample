﻿using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(ILogger<HomeController> logger, IProductService productService, ICartService cartService, ICouponService couponService)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
            _couponService = couponService;
        }

        public async Task<IActionResult> CartIndex()
        {

            return View(await FindUserCart());
        }

        [HttpPost]
        [ActionName("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
        

            var response = await _cartService.ApplyCoupon(model, accessToken);
            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        [ActionName("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon(CartViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;

            var response = await _cartService.RemoveCoupon(userId, accessToken);
            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        public async Task<IActionResult> Remove(int Id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _cartService.RemoveFromCart(Id, accessToken);
            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CartViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _cartService.Checkout(model.CartHeader, accessToken);
            if(response != null && response.GetType() == typeof(string))
            {
                TempData["Error"] = response;
                return RedirectToAction(nameof(Checkout));
            }else if (response != null)
            {
                return RedirectToAction(nameof(Confirmation));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            return View(await FindUserCart());
        }


        private async Task<CartViewModel> FindUserCart()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;

            var response = await _cartService.FindCartByUserID(userId, accessToken);
            if (response?.CartHeader != null)
            {
                if (!string.IsNullOrEmpty(response.CartHeader.CouponCode))
                {
                    var coupon = await _couponService.GetCoupon(response.CartHeader.CouponCode, accessToken);
                    if(coupon?.CouponCode != null)
                    {
                        response.CartHeader.DiscountTotal = coupon.DiscountAmount;
                    }
                }
                foreach (var detail in response.CartDetails)
                {
                    response.CartHeader.PurchaseAmount += (detail.Product.Price * detail.Count);
                }
                response.CartHeader.PurchaseAmount -= response.CartHeader.DiscountTotal;
            }
            return response;
        }


    }
}
