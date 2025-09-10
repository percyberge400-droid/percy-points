using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;

namespace POSPRA.Application.Services.FiscalService
{
    public interface IFiscalService
    {
        Task<ApiResponse<Invoice>> CreateAsync(Invoice invoice);
    }
}
