using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.BaseRepository
{
    /// <summary>
    /// SQLite-specific generic repository implementation.
    /// <para>
    /// Inherits all CRUD functionality from the base <see cref="Repository{T}"/> 
    /// and implements <see cref="IRepository{T}"/> for consistency with the repository pattern.
    /// </para>
    /// <para>
    /// Use this repository for entities stored in the SQLite database.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public class SqliteRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqliteRepository{T}"/>
        /// using the provided <see cref="SqliteDbContext"/>.
        /// </summary>
        /// <param name="context">The SQLite database context.</param>
        public SqliteRepository(SqliteDbContext context) : base(context) { }
    }
}
