namespace Pos.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Persists all pending changes to the underlying database.
        /// Returns the number of affected rows.
        /// </summary>
        Task<int> SaveChangesAsync();
    }

    public interface ISqlServerUnitOfWork : IUnitOfWork { }

    public interface ISqliteUnitOfWork : IUnitOfWork { }

}