using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface ICloudLogsRepository
    {
        Task AddRangeAsync(List<Logs> logs, string env);
        Task<IEnumerable<Logs>> GetAllLogsAsync(long posId, string env);
    }
}
