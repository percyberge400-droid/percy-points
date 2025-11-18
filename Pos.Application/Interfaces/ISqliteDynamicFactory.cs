namespace Pos.Application.Interfaces
{
    public interface ISqliteDynamicFactory
    {
        (IRepository<T> repo, IUnitOfWork uow) Create<T>(string dbPath) where T : class;
    }
}
