using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.BaseRepository
{
    // 🔹 SQL Server specific repository
    public class SqlServerRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        public SqlServerRepository(SqlServerDbContext context) : base(context) { }
    }
}