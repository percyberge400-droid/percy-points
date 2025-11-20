using Pos.Application.DTOs;
using Pos.Application.DTOs.InvoiceDtos;

namespace Pos.Application.Services.InvoiceService
{
    public interface IInvoiceService
    {
        Task<ApiResponse<InvoiceDto>> GetInvoiceWithItems(string invoiceNumber);
        Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto,string environment);
    }
}