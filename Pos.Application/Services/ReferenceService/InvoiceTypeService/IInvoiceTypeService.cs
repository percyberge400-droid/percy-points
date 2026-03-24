using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;

namespace Pos.Application.Services.ReferenceService.InvoiceTypeService
{
    public interface IInvoiceTypeService
    {
        Task SyncInvoiceTypesAsync(string env, IEnumerable<ReferenceDto> serverInvoiceTypes);
        Task<ApiResponse<List<ReferenceDto>>> GetInvoiceTypesAsync();
        Task<ApiResponse<int>> GetInvoiceTypesCountAsync();
    }
}
