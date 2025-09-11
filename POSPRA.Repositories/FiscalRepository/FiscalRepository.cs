using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.FiscalRepository
{
    // SQLite User repository
    public class FiscalRepository : SqliteRepository<FileRecord>, IFiscalRepository
    {
        public FiscalRepository(SqliteDbContext context) : base(context) { }
    }
}