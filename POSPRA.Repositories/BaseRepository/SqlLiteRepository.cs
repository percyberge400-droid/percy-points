using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.BaseRepository
{
    // 🔹 SQLite specific repository
    public class SqliteRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        public SqliteRepository(SqliteDbContext context) : base(context) { }
    }
}
