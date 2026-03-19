using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IServiceRenderedRepository
    {
        Task<IEnumerable<ServiceRendered>> GetServiceRendered(string env);
    }
}
