using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Infrastructure.Persistence.Repositories;

public class SqlServerRepositoryFactory : ISqlServerRepositoryFactory
{
    private readonly SqlServerDbContext _context;
    public SqlServerRepositoryFactory(SqlServerDbContext context) => _context = context;

    public IRepository<T> CreateRepository<T>() where T : class => new Repository<T>(_context);
}
