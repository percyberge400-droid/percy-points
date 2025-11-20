using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces.Repositories;

namespace Pos.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        // Expose IQueryable for advanced queries
        public IQueryable<T> Query() => _dbSet.AsQueryable();

        // Get entity by primary key
        public async Task<T?> GetByIdAsync(object id) =>
            await _dbSet.FindAsync(id);

        // Get all entities
        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _dbSet.AsNoTracking().ToListAsync();

        // Find entities by predicate
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        // Add single entity
        public async Task AddAsync(T entity) =>
            await _dbSet.AddAsync(entity);

        // Add multiple entities
        public async Task AddRangeAsync(IEnumerable<T> entities) =>
            await _dbSet.AddRangeAsync(entities);

        // Update single entity
        public Task UpdateAsync(T entity)
        {
            DetachIfTracked(entity);
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        // Remove single entity
        public void Remove(T entity) => _dbSet.Remove(entity);

        // Remove multiple entities
        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any()) return;

            foreach (var entity in entities)
            {
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    var trackedEntity = FindTrackedEntity(entity);
                    if (trackedEntity != null)
                    {
                        _dbSet.Remove(trackedEntity);
                        continue;
                    }
                    _dbSet.Attach(entity);
                }

                _dbSet.Remove(entity);
            }
        }

        // Count entities optionally filtered by predicate
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        // Check if any entity exists matching predicate
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        // Get first entity or default
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(predicate);
            if (entity != null)
            {
                await _context.Entry(entity).ReloadAsync();
            }
            return entity;
        }

        // Update multiple entities
        public void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any()) return;

            foreach (var entity in entities)
            {
                DetachIfTracked(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        // Execute raw SQL or stored procedure (non-query)
        public async Task<int> ExecuteProcedureAsync(string procedureName, params object[] parameters) =>
            await _context.Database.ExecuteSqlRawAsync(procedureName, parameters);

        // Execute raw SQL or stored procedure (returns entities)
        public async Task<List<TResult>> QueryProcedureAsync<TResult>(string procedureName, params object[] parameters) where TResult : class =>
            await _context.Set<TResult>().FromSqlRaw(procedureName, parameters).ToListAsync();

        #region Helpers

        // Detach entity if it's already being tracked to prevent duplicate tracking issues
        private void DetachIfTracked(T entity)
        {
            var trackedEntity = FindTrackedEntity(entity);
            if (trackedEntity != null)
            {
                _context.Entry(trackedEntity).State = EntityState.Detached;
            }
        }

        // Find a tracked entity with the same primary key
        private T? FindTrackedEntity(T entity)
        {
            var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
            if (key == null) return null;

            var keyValues = key.Properties
                .Select(p => typeof(T).GetProperty(p.Name)?.GetValue(entity))
                .ToArray();

            return _context.ChangeTracker.Entries<T>()
                .FirstOrDefault(e => key.Properties
                    .Select(p => e.Property(p.Name).CurrentValue)
                    .SequenceEqual(keyValues))?.Entity;
        }

        #endregion
    }
}
