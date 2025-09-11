using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.LogRepository
{
    // SQLite User repository
    public class LogRepository : SqliteRepository<Logs>, ILogRepository
    {
        public LogRepository(SqliteDbContext context) : base(context) { }
    }
}
