using Pos.Application.Interfaces.Repositories;

namespace Pos.Application.Interfaces
{
    public interface ISqlServerRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
    }
}
