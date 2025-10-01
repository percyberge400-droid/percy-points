using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.ProductCatalogDtos;

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

        /// <summary>
        /// Updates multiple <see cref="FileRecordDto"/> objects in the database.
        /// </summary>
        /// <param name="fileRecordDtos">
        /// A list of <see cref="FileRecordDto"/> instances to update.  
        /// Must not be <c>null</c> or empty.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the list of updated
        /// <see cref="FileRecordDto"/> objects if the operation succeeds,
        /// or an error response if validation fails or no records are updated.
        /// </returns>
        Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos, bool isSingle);

        /// <summary>
        /// This method is used to create product catalogue in SQLite.
        /// </summary>
        /// <param name="productCatalogueDto"></param>
        /// <returns></returns>
        Task<ApiResponse<ProductCatalogueDto>> PostProductCatalog(ProductCatalogueDto productCatalogueDto);

        /// <summary>
        /// This API is used to get product catalog.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<List<ProductCatalogueDto>>> GetProductCatalogue();
    }
}