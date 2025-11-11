using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces;

namespace Pos.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        protected readonly TContext _context;

        /// <summary>
        /// Expose the DbContext publicly so repositories can reuse it.
        /// </summary>
        public TContext DbContext => _context;

        /// <summary>
        /// Creates a new UnitOfWork instance for the specified DbContext.
        /// </summary>
        public UnitOfWork(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class SqlServerUnitOfWork : UnitOfWork<SqlServerDbContext>, ISqlServerUnitOfWork
    {
        public SqlServerUnitOfWork(SqlServerDbContext context) : base(context) { }
    }

    public class SqliteUnitOfWork : UnitOfWork<SqliteDbContext>, ISqliteUnitOfWork
    {
        public SqliteUnitOfWork(SqliteDbContext context) : base(context) { }
    }
}
