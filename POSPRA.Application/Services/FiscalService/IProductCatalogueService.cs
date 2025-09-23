using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.Application.Services.FiscalService
{
    public interface IProductCatalogueService
    {
        Task<ApiResponse<List<ProductCatalogueDTO>>> GetAllAsync();
    }
}