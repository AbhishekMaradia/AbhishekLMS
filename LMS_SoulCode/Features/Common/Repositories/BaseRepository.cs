using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.Common.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS_SoulCode.Features.Common.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly LmsDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(LmsDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default) => 
            await _dbSet.ToListAsync(cancellationToken);

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => 
            await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                if (entity is ISoftDelete softDelete)
                {
                    softDelete.IsDeleted = true;
                    _dbSet.Update(entity);
                }
                else
                {
                    _dbSet.Remove(entity);
                }
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) => 
            await GetByIdAsync(id, cancellationToken) != null;

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await _context.SaveChangesAsync(cancellationToken);

        // PROTECTED helper methods for derived repositories to use
        // This avoids interface implementation issues while providing shared logic
        protected virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? queryModifier = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (queryModifier != null) query = queryModifier(query);
            if (filter != null) query = query.Where(filter);

            return await query.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        protected virtual async Task<(IEnumerable<TResult> Items, int TotalCount)> GetPagedAsync<TResult>(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<TResult>>? projection = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null) query = query.Where(filter);

            if (projection != null)
            {
                return await projection(query).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
            }

            // Fallback: If no projection, TResult must be compatible with T
            return await ((IQueryable<TResult>)query).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
