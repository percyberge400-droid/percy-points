using Pos.Application.Interfaces.Repositories;

namespace Pos.Application.Interfaces
{
    public interface ISqliteRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
    }
}
