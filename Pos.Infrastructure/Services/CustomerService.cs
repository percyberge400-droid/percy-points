using Pos.Application.Interfaces;
using Pos.Domain.Entities;

public class CustomerService : ICustomerService
{
    private readonly IRepositoryFactory _repositoryFactory;

    public CustomerService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public PosClients GetAllAsync(long id)
    {
        var repo = _repositoryFactory.CreateRepository<PosClients>();
        return  repo.GetAllAsync().Result.FirstOrDefault(x => x.POSRegistrationNumber == id);
    }

}
