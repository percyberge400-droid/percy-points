using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.FiscalRepository
{
    /// <summary>
    /// Concrete repository for managing <see cref="FileRecord"/> entities
    /// in the SQLite database.
    /// <para>
    /// Inherits all CRUD functionality from <see cref="SqliteRepository{T}"/>
    /// and implements <see cref="IFiscalRepository"/> for any
    /// fiscal-specific queries or commands.
    /// </para>
    /// </summary>
    public class FiscalRepository : SqliteRepository<FileRecord>, IFiscalRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FiscalRepository"/> class
        /// using the provided <see cref="SqliteDbContext"/>.
        /// </summary>
        /// <param name="context">The SQLite database context.</param>
        public FiscalRepository(SqliteDbContext context) : base(context) { }
    }
}
