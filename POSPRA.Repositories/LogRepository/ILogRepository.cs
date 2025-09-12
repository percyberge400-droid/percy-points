using POSPRA.Domain.Entities;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.LogRepository
{
    /// <summary>
    /// Specialized repository interface for working with <see cref="Logs"/> entities.
    /// Inherits all generic CRUD operations from <see cref="IRepository{T}"/>.
    /// Implement this interface when you need custom queries or commands
    /// specific to the Logs table.
    /// </summary>
    public interface ILogRepository : IRepository<Logs>
    {
        // Add any Log-specific repository methods here if needed,
        // e.g., Task<IEnumerable<Logs>> GetRecentLogsAsync(int count);
    }
}
