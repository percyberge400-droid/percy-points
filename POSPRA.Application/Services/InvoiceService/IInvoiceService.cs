using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.Application.Services.InvoiceService
{
    public interface IInvoiceService
    {
        Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto);
    }
}