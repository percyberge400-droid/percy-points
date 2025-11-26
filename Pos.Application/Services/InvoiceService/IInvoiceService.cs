using Pos.Application.DTOs;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.InvoiceDTOs;

namespace Pos.Application.Services.InvoiceService
{
    public interface IInvoiceService
    {
        Task<ApiResponse<InvoiceDto>> GetInvoiceWithItems(string invoiceNumber);
        Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto,string environment);
        Task<InvoiceResponseDto> OldCreateAsync(InvoiceDto dto,string environment);
    }
}