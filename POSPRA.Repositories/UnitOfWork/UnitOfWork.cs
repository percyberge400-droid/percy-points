using Microsoft.EntityFrameworkCore;
using POSPRA.Infrastructure.Context;

namespace POSPRA.Repositories.UnitOfWork
{
    // 🔹 Shared interface for any UnitOfWork
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        Task<int> SaveChangesAsync();
    }

    // 🔹 Generic base UnitOfWork
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        private readonly TContext _context;

        public UnitOfWork(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    // 🔹 SQLite-specific UnitOfWork
    public class SqliteUnitOfWork : UnitOfWork<SqliteDbContext>, ISqliteUnitOfWork
    {
        public SqliteUnitOfWork(SqliteDbContext context) : base(context) { }
    }

    // 🔹 SQL Server-specific UnitOfWork
    public class SqlServerUnitOfWork : UnitOfWork<SqlServerDbContext>, ISqlServerUnitOfWork
    {
        public SqlServerUnitOfWork(SqlServerDbContext context) : base(context) { }
    }
}
