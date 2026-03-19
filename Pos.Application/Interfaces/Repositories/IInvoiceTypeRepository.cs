using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IInvoiceTypeRepository
    {
        Task<IEnumerable<InvoiceType>> GetAllInvoiceType(string env);
    }
}

