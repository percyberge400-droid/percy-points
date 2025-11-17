namespace Pos.Application.Interfaces
{
    public interface ISqliteRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
        IRepository<T> CreateRepository<T>(string dbPath) where T : class;
        ISqliteUnitOfWork CreateUnitOfWork(string dbPath);
    }
}
