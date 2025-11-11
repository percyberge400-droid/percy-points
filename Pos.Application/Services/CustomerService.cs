using Domain.Entities;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;

public class CustomerService : ICustomerService
{
    private readonly IRepository<PosClients> _sqlRepo;
    private readonly IRepository<FileRecord> _sqliteRepo;

    public CustomerService(
        ISqlServerRepositoryFactory sqlRepoFactory,
        ISqliteRepositoryFactory sqliteRepoFactory)
    {
        _sqlRepo = sqlRepoFactory.CreateRepository<PosClients>();
        _sqliteRepo = sqliteRepoFactory.CreateRepository<FileRecord>();
    }

    public async Task<PosClients?> GetByIdFromSqlAsync(long id)
    {
        var clients = await _sqlRepo.GetAllAsync();
        return clients.FirstOrDefault(x => x.POSRegistrationNumber == id);
    }

    public async Task<FileRecord?> GetByIdFromSqliteAsync(long id)
    {
        var clients = await _sqliteRepo.GetAllAsync();
        return clients.FirstOrDefault(x => x.ID == id);
    }
}
