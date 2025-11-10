using Domain.Entities;
using Pos.Domain.Entities;

namespace Pos.Application.Services
{
    public interface ICustomerService
    {
        Task<PosClients?> GetByIdFromSqlAsync(long id);
        Task<FileRecord?> GetByIdFromSqliteAsync(long id);
    }
}