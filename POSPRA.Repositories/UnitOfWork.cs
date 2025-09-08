using POSPRA.Infrastructure.Context;

namespace POSPRA.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SqliteDbContext _context;

        public UnitOfWork(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
