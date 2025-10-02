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
    /// and implements <see cref="ILogSQLiteRepository"/> for possible
    /// log-specific queries or commands.
    /// </para>
    /// </summary>
    public class LogSQLiteRepository : SqliteRepository<Logs>, ILogSQLiteRepository
    {
        /// <summary>
        /// Creates a new instance of <see cref="LogRepository"/> 
        /// using the provided <see cref="SqliteDbContext"/>.
        /// </summary>
        /// <param name="context">SQLite DbContext for POSPRA.</param>
        public LogSQLiteRepository(SqliteDbContext context) : base(context) { }
    }

    public class LogSQLServerRepository : SqlServerRepository<Logs>, ILogSQLServerRepository
    {
        /// <summary>
        /// Creates a new instance of <see cref="LogRepository"/> 
        /// using the provided <see cref="SqlServerDbContext"/>.
        /// </summary>
        /// <param name="context">SQLSERVER DbContext for POSPRA.</param>
        public LogSQLServerRepository(SqlServerDbContext context) : base(context) { }
    }
}
