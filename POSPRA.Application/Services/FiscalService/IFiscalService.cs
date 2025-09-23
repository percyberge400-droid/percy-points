using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Service contract for fiscal operations such as creating invoices.
    /// </summary>
    public interface IFiscalService
    {
        /// <summary>
        /// Retrieves all file records from the data store.
        /// </summary>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of all
        /// <see cref="FileRecordDTO"/> objects.
        /// </returns>
        Task<ApiResponse<List<FileRecordDto>>> GetAllAsync();

        /// <summary>
        /// Retrieves only the file records that have **not** been synced yet.
        /// </summary>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of unsynced
        /// <see cref="FileRecordDTO"/> objects.
        /// </returns>
        Task<ApiResponse<List<FileRecordDto>>> GetAllUnsyncedAsync();

        /// <summary>
        /// Creates a new invoice record asynchronously.
        /// </summary>
        /// <param name="InvoiceDto">The InvoiceDto Dto to create.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created <see cref="Invoice"/> and operation status.
        /// </returns>
        Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto);

        Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos);
    }
}