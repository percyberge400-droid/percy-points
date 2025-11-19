using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;

public class CustomerService : ICustomerService
{
    private readonly ISqlServerRepositoryFactory _sqlRepoFactory;
    private readonly ISqliteRepositoryFactory _sqliteRepoFactory;
    private readonly IEnvironmentService _environmentService;

    public CustomerService(
        ISqlServerRepositoryFactory sqlRepoFactory,
        ISqliteRepositoryFactory sqliteRepoFactory,
        IEnvironmentService environmentService)
    {
        _sqlRepoFactory = sqlRepoFactory;
        _sqliteRepoFactory = sqliteRepoFactory;
        _environmentService = environmentService;
    }

    /// <summary>
    /// Get PosClients from SQL Server based on process-specific environment
    /// </summary>
    /// <param name="id">POSRegistrationNumber</param>
    public async Task<PosClients?> GetByIdFromSqlAsync(long id)
    {
        // Get current environment for this process/user
        var env = _environmentService.GetCurrentEnvironmentAsync();

        // TODO: Optionally switch repository/connection based on env
        var repo = _sqlRepoFactory.CreateRepository<PosClients>();
        var clients = await repo.GetAllAsync();

        return clients.FirstOrDefault(x => x.POSRegistrationNumber == id);
    }

    /// <summary>
    /// Get FileRecord from SQLite (usually local DB/Sandbox)
    /// </summary>
    /// <param name="id">FileRecord ID</param>
    public async Task<FileRecord?> GetByIdFromSqliteAsync(long id)
    {
        var env = _environmentService.GetCurrentEnvironmentAsync();

        var repo = _sqliteRepoFactory.CreateRepository<FileRecord>();
        var clients = await repo.GetAllAsync();

        return clients.FirstOrDefault(x => x.ID == id);
    }
}
