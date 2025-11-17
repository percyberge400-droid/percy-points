// SQLite
using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces;
using Pos.Infrastructure.Persistence.Repositories;

public class SqliteRepositoryFactory : ISqliteRepositoryFactory
{
    private readonly SqliteDbContext _context;
    public SqliteRepositoryFactory(SqliteDbContext context) => _context = context;

    public IRepository<T> CreateRepository<T>() where T : class => new Repository<T>(_context);

    public ISqliteUnitOfWork CreateUnitOfWork(string dbPath)
    {
        var context = CreateDbContext(dbPath);
        return new SqliteUnitOfWork(context);
    }

    public IRepository<T> CreateRepository<T>(string dbPath) where T : class
    {
        var context = CreateDbContext(dbPath);
        return new Repository<T>(context);
    }

    private SqliteDbContext CreateDbContext(string dbPath)
    {
        var options = new DbContextOptionsBuilder<SqliteDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new SqliteDbContext(options);
    }
}