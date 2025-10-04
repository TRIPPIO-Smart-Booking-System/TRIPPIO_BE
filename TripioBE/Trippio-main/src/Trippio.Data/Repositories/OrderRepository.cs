using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Data.SeedWorks;

namespace Trippio.Data.Repositories
{
    public class OrderRepository : RepositoryBase<Order, int>, IOrderRepository
    {
        private readonly TrippioDbContext _context;

        public OrderRepository(TrippioDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Order> Query()
        {
            return _context.Set<Order>().AsNoTracking();
        }

        public async Task<Order?> FindByIdAsync(int id)
        {
            return await _context.Set<Order>().FirstOrDefaultAsync(o => o.Id == id);
        }

        public void Update(Order entity)
        {
            _context.Set<Order>().Update(entity);
        }
    }
}
