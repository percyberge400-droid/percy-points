using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace POSPRA.Repositories.BaseRepository.Repository
{
    /// <summary>
    /// Generic repository base class that provides standard CRUD operations
    /// and stored procedure execution for any EF Core entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        public IQueryable<T> Query() => _dbSet.AsQueryable();

        /// <summary>
        /// The DbContext instance used by this repository.
        /// </summary>
        protected readonly DbContext _context;

        /// <summary>
        /// The DbSet for the entity type <typeparamref name="T"/>.
        /// </summary>
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of <see cref="Repository{T}"/>.
        /// </summary>
        /// <param name="context">The EF Core DbContext.</param>
        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(object id) =>
            await _dbSet.FindAsync(id);

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _dbSet.AsNoTracking().ToListAsync();

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        /// <inheritdoc/>
        public async Task AddAsync(T entity) =>
            await _dbSet.AddAsync(entity);

        /// <inheritdoc/>
        public async Task AddRangeAsync(IEnumerable<T> entities) =>
            await _dbSet.AddRangeAsync(entities);

        /// <inheritdoc/>
        public Task UpdateAsync(T entity)
        {
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey();
            if (key == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a primary key defined.");

            var keyValues = key.Properties
                .Select(p => typeof(T).GetProperty(p.Name).GetValue(entity))
                .ToArray();

            var trackedEntity = _context.ChangeTracker.Entries<T>()
                .FirstOrDefault(e => key.Properties
                    .Select(p => e.Property(p.Name).CurrentValue)
                    .SequenceEqual(keyValues));

            if (trackedEntity != null)
            {
                trackedEntity.State = EntityState.Detached;
            }

            _dbSet.Update(entity);

            // No real async operation here, so just return a completed Task
            return Task.CompletedTask;
        }
        /// <inheritdoc/>
        public void Remove(T entity) =>
            _dbSet.Remove(entity);

        /// <inheritdoc/>
        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return;

            foreach (var entity in entities)
            {
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    // Try to find the already tracked entity with the same key
                    var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
                    if (key != null)
                    {
                        var keyValues = key.Properties
                            .Select(p => typeof(T).GetProperty(p.Name)?.GetValue(entity))
                            .ToArray();

                        var trackedEntity = _context.ChangeTracker.Entries<T>()
                            .FirstOrDefault(e => key.Properties
                                .Select(p => e.Property(p.Name).CurrentValue)
                                .SequenceEqual(keyValues));

                        if (trackedEntity != null)
                        {
                            // If an instance is already tracked, remove that one instead
                            _dbSet.Remove(trackedEntity.Entity);
                            continue;
                        }
                    }

                    // If no tracked instance found, attach and remove
                    _dbSet.Attach(entity);
                }

                _dbSet.Remove(entity);
            }
        }


        /// <inheritdoc/>
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        /// <inheritdoc/>
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
           await _dbSet.FirstOrDefaultAsync(predicate);

        /// <summary>
        /// Updates a collection of entities in a single call.
        /// Marks all entities as Modified so they are persisted on SaveChanges.
        /// </summary>
        /// <param name="entities">Entities to update.</param>
        public void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return;

            var entityType = typeof(T);
            var key = _context.Model.FindEntityType(entityType)?.FindPrimaryKey();
            if (key == null)
                throw new InvalidOperationException($"Entity {entityType.Name} does not have a primary key defined.");

            foreach (var entity in entities)
            {
                var keyValues = key.Properties
                    .Select(p => entityType.GetProperty(p.Name)?.GetValue(entity))
                    .ToArray();

                // find if this entity key is already tracked
                var trackedEntity = _context.ChangeTracker.Entries<T>()
                    .FirstOrDefault(e => key.Properties
                        .Select(p => e.Property(p.Name).CurrentValue)
                        .SequenceEqual(keyValues));

                if (trackedEntity != null)
                {
                    // detach old tracked version before updating
                    trackedEntity.State = EntityState.Detached;
                }

                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Executes a raw SQL command or stored procedure that does not return rows.
        /// </summary>
        /// <param name="procedureName">The SQL or procedure name to execute.</param>
        /// <param name="parameters">Optional parameters for the command.</param>
        /// <returns>The number of affected rows.</returns>
        public async Task<int> ExecuteProcedureAsync(string procedureName, params object[] parameters) =>
            await _context.Database.ExecuteSqlRawAsync(procedureName, parameters);

        /// <summary>
        /// Executes a stored procedure or raw SQL query that returns a result set
        /// and maps it to the specified <typeparamref name="TResult"/> type.
        /// TResult must be registered as an entity or keyless type in the DbContext.
        /// </summary>
        /// <typeparam name="TResult">The type to map the results to.</typeparam>
        /// <param name="procedureName">The SQL or procedure name to execute.</param>
        /// <param name="parameters">Optional parameters for the query.</param>
        /// <returns>A list of <typeparamref name="TResult"/> objects.</returns>
        public async Task<List<TResult>> QueryProcedureAsync<TResult>(string procedureName, params object[] parameters) where TResult : class =>
            await _context.Set<TResult>().FromSqlRaw(procedureName, parameters).ToListAsync();
    }
}
