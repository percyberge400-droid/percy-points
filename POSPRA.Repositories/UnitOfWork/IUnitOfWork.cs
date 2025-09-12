namespace POSPRA.Repositories.UnitOfWork
{
    /// <summary>
    /// Unit of Work contract for operations that target the SQLite database.
    /// <para>
    /// Use this interface when you need to commit or rollback
    /// changes made through repositories that point to the SQLite context.
    /// </para>
    /// </summary>
    public interface ISqliteUnitOfWork : IUnitOfWork { }

    /// <summary>
    /// Unit of Work contract for operations that target the SQL Server database.
    /// <para>
    /// Use this interface when you need to commit or rollback
    /// changes made through repositories that point to the SQL Server context.
    /// </para>
    /// </summary>
    public interface ISqlServerUnitOfWork : IUnitOfWork { }
}