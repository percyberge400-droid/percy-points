using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Service contract for fiscal operations such as creating invoices.
    /// </summary>
    public interface IFiscalService
    {
        /// <summary>
        /// Creates a new invoice record asynchronously.
        /// </summary>
        /// <param name="InvoiceDto">The InvoiceDto Dto to create.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created <see cref="Invoice"/> and operation status.
        /// </returns>
        Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto);
    }
}
