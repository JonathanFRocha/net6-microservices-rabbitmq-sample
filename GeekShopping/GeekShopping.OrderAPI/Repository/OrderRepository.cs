
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Model.Context;
using GeekShopping.OrderAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<MySQLContext> _context;

        public OrderRepository(DbContextOptions<MySQLContext> context)
        {
            _context = context;
        }

        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            if (orderHeader == null) return false;
            await using var _db = new MySQLContext(_context);
            _db.OrderHeaders.Add(orderHeader);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateOrderPaymentStatus(long orderHeaderId, bool paidStatus)
        {
            await using var _db = new MySQLContext(_context);
            var header = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.Id == orderHeaderId);
            if (header != null)
            {
                header.PaymentStatus = paidStatus;
                await _db.SaveChangesAsync();
            }
        }
    }
}
