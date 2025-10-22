using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class, IEntity<TKey>
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes
        )
        {
            IQueryable<T> query = _dbSet;

            // Include navigation property nếu có
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task AddRangeAsync(IEnumerable<T> items)
        {
            await _dbSet.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<List<T>> GetAllByIdsAsync(IEnumerable<TKey> ids, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            // Chỉ hỗ trợ property tên "Id"
            query = query.Where(e => ids.Contains(EF.Property<TKey>(e, "Id")));
            return await query.ToListAsync();
        }
        public async Task<T?> GetByIdAsync(TKey id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entity = await query.SingleOrDefaultAsync(e => e.Id.Equals(id));

            if (entity == null)
                throw new AppException(ErrorCode.NotFound, "Entity not found");

            return entity;
        }
    }
}
