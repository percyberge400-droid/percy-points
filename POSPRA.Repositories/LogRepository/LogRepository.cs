using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.LogRepository
{
    /// <summary>
    /// Concrete repository for managing <see cref="Logs"/> entities
    /// in the SQLite database.
    /// <para>
    /// Inherits all CRUD functionality from <see cref="SqliteRepository{T}"/>
    /// and implements <see cref="ILogRepository"/> for possible
    /// log-specific queries or commands.
    /// </para>
    /// </summary>
    public class LogRepository : SqliteRepository<Logs>, ILogRepository
    {
        /// <summary>
        /// Creates a new instance of <see cref="LogRepository"/> 
        /// using the provided <see cref="SqliteDbContext"/>.
        /// </summary>
        /// <param name="context">SQLite DbContext for POSPRA.</param>
        public LogRepository(SqliteDbContext context) : base(context) { }
    }
}
