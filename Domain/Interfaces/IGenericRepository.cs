using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGenericRepository<T, TKey> where T : IEntity<TKey>
    {
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes
        );
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> items);
        Task<T> UpdateAsync(T entity);
        Task RemoveAsync(T entity);
        Task<List<T>> GetAllByIdsAsync(IEnumerable<TKey> ids, params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(TKey id, params Expression<Func<T, object>>[] includes);
    }
}
