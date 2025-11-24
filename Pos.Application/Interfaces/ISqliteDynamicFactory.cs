using Pos.Application.Interfaces.Repositories;

namespace Pos.Application.Interfaces
{
    public interface ISqliteDynamicFactory
    {
        (IRepository<T> repo, IUnitOfWork uow) Create<T>(string dbPath, string password) where T : class;
    }
}