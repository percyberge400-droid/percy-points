using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.Application.Services.LiveService
{
    /// <summary>
    /// Defines operations for handling live invoice data,
    /// including decrypting & saving invoices and exporting them to CSV.
    /// </summary>
    public interface ILiveService
    {
        /// <summary>
        /// Decrypts a list of encrypted file records and saves
        /// the contained invoices (and their items) to the database.
        /// </summary>
        /// <param name="dto">
        /// A collection of <see cref="FileRecordDto"/> objects containing
        /// encrypted invoice data to be processed and saved.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{FileRecordDto}"/> indicating whether
        /// the operation succeeded or failed, along with optional details.
        /// </returns>
        Task<ApiResponse<List<FileRecordDto>>> DecryptAndSaveInvoicesAsync(List<FileRecordDto> dto);

        /// <summary>
        /// Retrieves invoices filtered by the specified criteria
        /// and returns them as a CSV-formatted string.
        /// </summary>
        /// <param name="dto">
        /// The filter options (POS ID, date range, etc.) for selecting invoices.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{String}"/> containing the CSV data of the filtered invoices.
        /// </returns>
        Task<ApiResponse<string>> GetInvoicesCsvAsync(InvoiceFilterDto dto);

        Task<ApiResponse<Invoice>> CreateInvoiceWithItemsAsync(InvoiceDto dto);
    }
}