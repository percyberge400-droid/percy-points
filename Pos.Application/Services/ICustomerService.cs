using Pos.Domain.Entities;

namespace Pos.Application.Services
{
    public interface ICustomerService
    {
        /// <summary>
        /// Get PosClients from SQL Server based on the current process environment
        /// </summary>
        /// <param name="id">POSRegistrationNumber</param>
        Task<PosClients?> GetByIdFromSqlAsync(long id);

        /// <summary>
        /// Get FileRecord from SQLite based on the current process environment
        /// </summary>
        /// <param name="id">FileRecord ID</param>
        Task<FileRecord?> GetByIdFromSqliteAsync(long id);
    }
}