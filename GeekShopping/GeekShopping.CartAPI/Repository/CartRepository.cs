using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly MySQLContext _context;
        private readonly IMapper _mapper;

        public CartRepository(MySQLContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            var cartHeader = await _context.CartHeaders
                        .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cartHeader != null)
            {
                cartHeader.CouponCode = couponCode;
                _context.CartHeaders.Update(cartHeader);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeader = await _context.CartHeaders
                                    .FirstOrDefaultAsync(c => c.UserId == userId);
            if(cartHeader != null)
            {
                _context.CartDetails.RemoveRange(_context.CartDetails.Where(c => c.CartHeaderId == cartHeader.Id));
                _context.CartHeaders.Remove(cartHeader);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
           
        }

        public async Task<CartVO> FindCartByUserId(string userId)
        {
            var cart = new Cart()
            {
                CartHeader = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId) ?? new CartHeader(),
            };

            cart.CartDetails = _context.CartDetails.Where(c => c.CartHeaderId == cart.CartHeader.Id)
                .Include(c => c.Product);

            return _mapper.Map<CartVO>(cart);

        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            var cartHeader = await _context.CartHeaders
            .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cartHeader != null)
            {
                cartHeader.CouponCode = "";
                _context.CartHeaders.Update(cartHeader);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveFromCart(long CartDetailsId)
        {
            try
            {
                CartDetail cartDetail = await _context.CartDetails.FirstOrDefaultAsync(c => c.Id == CartDetailsId);
                var total = _context.CartDetails.Where(c => c.CartHeaderId == cartDetail.CartHeaderId).Count();

                _context.CartDetails.Remove(cartDetail);
                if(total == 1)
                {
                    var cartHeaderToRemove = await _context.CartHeaders.FirstOrDefaultAsync(c => c.Id == cartDetail.CartHeaderId);
                    _context.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<CartVO> SaveOrUpdateCart(CartVO cartVO)
        {
            var cart = _mapper.Map<Cart>(cartVO);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == cartVO.CartDetails.FirstOrDefault().ProductId);

            if (product == null)
            {
                _context.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _context.SaveChangesAsync();
            }

            var cartHeader = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == cart.CartHeader.UserId);
            if (cartHeader == null)
            {
                _context.CartHeaders.Add(cart.CartHeader);
                await _context.SaveChangesAsync();
                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
                cart.CartDetails.FirstOrDefault().Product = null;
                _context.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _context.SaveChangesAsync();
            }
            else
            {
                var cartDetail = await _context.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    p => p.ProductId == cart.CartDetails.FirstOrDefault().ProductId && p.CartHeaderId == cartHeader.Id
                    );
                if (cartDetail == null)
                {
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeader.Id;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _context.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _context.SaveChangesAsync();
                }
                else
                {
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetail.Count;
                    cart.CartDetails.FirstOrDefault().Id = cartDetail.Id;
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetail.CartHeaderId;
                    _context.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _context.SaveChangesAsync();
                }

            }


            return _mapper.Map<CartVO>(cart);
        }
    }
}
