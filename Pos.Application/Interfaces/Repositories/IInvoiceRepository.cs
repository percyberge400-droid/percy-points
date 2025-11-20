using Pos.Application.DTOs.InvoiceDtos;
using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetInvoicesAsync(InvoiceFilterDto dto, string env);
        Task AddAsync(Invoice invoice, string env);
    }
}
