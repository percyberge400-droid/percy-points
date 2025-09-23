using System.Linq.Expressions;

namespace POSPRA.Repositories.BaseRepository.Repository
{
    /// <summary>
    /// Generic repository contract defining standard CRUD operations
    /// and stored procedure execution for any entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Returns an IQueryable for building custom queries.
        /// No tracking is applied; call AsTracking() if needed.
        /// </summary>
        IQueryable<T> Query();

        /// <summary>
        /// Retrieves an entity by its primary key.
        /// </summary>
        Task<T?> GetByIdAsync(object id);

        /// <summary>
        /// Retrieves all entities of type <typeparamref name="T"/>.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Finds entities matching the specified predicate.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds a new entity to the context.
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Adds multiple entities to the context.
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity in the context.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Removes an entity from the context.
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// Removes multiple entities from the context.
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Returns the count of entities matching an optional predicate.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Checks if any entity exists that matches the specified predicate.
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retrieves the first entity matching the predicate or null if none found.
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Executes a stored procedure or raw SQL command that does not return a result set.
        /// Returns the number of rows affected.
        /// </summary>
        Task<int> ExecuteProcedureAsync(string procedureName, params object[] parameters);

        /// <summary>
        /// Executes a stored procedure or raw SQL query that returns a result set
        /// and maps it to <typeparamref name="TResult"/> objects.
        /// TResult can be an entity or a plain DTO.
        /// </summary>
        Task<List<TResult>> QueryProcedureAsync<TResult>(string procedureName, params object[] parameters)
            where TResult : class;
    }
}
