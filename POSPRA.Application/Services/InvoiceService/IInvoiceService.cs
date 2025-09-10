using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;

namespace POSPRA.Application.Services.InvoiceService
{
    public interface IInvoiceService
    {
        Task<ApiResponse<Invoice>> CreateAsync(Invoice invoice);
    }
}
