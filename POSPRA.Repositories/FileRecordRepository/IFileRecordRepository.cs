using POSPRA.Domain.Entities;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.FileRecordRepository
{
    /// <summary>
    /// Specialized repository interface for working with <see cref="FileRecord"/> entities.
    /// <para>
    /// Inherits all generic CRUD operations from <see cref="IRepository{T}"/>.
    /// Extend this interface to add fiscal-specific queries or commands
    /// related to the <c>FileRecord</c> table.
    /// </para>
    /// </summary>
    public interface IFileRecordRepository : IRepository<FileRecord>
    {
        // Add custom methods for fiscal operations if needed.
        // e.g., Task<IEnumerable<FileRecord>> GetByStatusAsync(string status);
    }
}
