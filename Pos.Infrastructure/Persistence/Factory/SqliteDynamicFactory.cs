using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Infrastructure.Persistence.Repositories;

namespace Pos.Infrastructure.Persistence.Factory
{
    public class SqliteDynamicFactory : ISqliteDynamicFactory
    {
        public (IRepository<T> repo, IUnitOfWork uow) Create<T>(string dbPath) where T : class
        {
            var options = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            var context = new SqliteDbContext(options);

            var uow = new SqliteUnitOfWork(context);
            var repo = new Repository<T>(context);

            return (repo, uow);
        }
    }
}
