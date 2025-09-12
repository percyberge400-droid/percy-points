using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;

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
        /// <param name="invoice">The invoice entity to create.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created <see cref="Invoice"/> and operation status.
        /// </returns>
        Task<ApiResponse<Invoice>> CreateAsync(Invoice invoice);
    }
}
