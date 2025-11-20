namespace Pos.Application.Interfaces.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
    }
}
