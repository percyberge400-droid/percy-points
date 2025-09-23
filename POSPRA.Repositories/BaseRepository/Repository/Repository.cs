using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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
            await _dbSet.ToListAsync();

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
        public void Update(T entity) =>
            _dbSet.Update(entity);

        /// <inheritdoc/>
        public void Remove(T entity) =>
            _dbSet.Remove(entity);

        /// <inheritdoc/>
        public void RemoveRange(IEnumerable<T> entities) =>
            _dbSet.RemoveRange(entities);

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
