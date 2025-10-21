using Trippio.Core.Domain.Entities;
using Trippio.Core.SeedWorks;

namespace Trippio.Data.Repositories
{
    public interface IOrderRepository : IRepository<Order, int>
    {
        IQueryable<Order> Query();                 
        Task<Order?> FindByIdAsync(int id);        
        void Update(Order entity);
        Task AddAsync(Order order);
    }
}
