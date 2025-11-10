// SQLite
using Pos.Application.Interfaces;
using Pos.Infrastructure.Persistence.Repositories;

public class SqliteRepositoryFactory : ISqliteRepositoryFactory
{
    private readonly SqliteDbContext _context;
    public SqliteRepositoryFactory(SqliteDbContext context) => _context = context;

    public IRepository<T> CreateRepository<T>() where T : class => new Repository<T>(_context);
}