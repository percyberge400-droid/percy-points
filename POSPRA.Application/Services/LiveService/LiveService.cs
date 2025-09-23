using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.LiveService
{
    /// <summary>
    /// Provides live operations such as decrypting incoming invoice data,
    /// saving invoices and their items to SQL Server, and exporting
    /// filtered invoices as CSV.
    /// </summary>
    public class LiveService(
        IOptions<AppSettings> options,
        SqlServerRepository<Invoice> invoiceRepository,
        SqlServerRepository<InvoiceItems> invoiceItemsRepository,
        IMapper mapper,
        ISqlServerUnitOfWork sqlServerUnitOfWork,
        ISqliteUnitOfWork sqliteUnitOfWork,
        IFiscalRepository fiscalRepository) : ILiveService
    {
        private readonly AppSettings _settings = options.Value;
        private readonly SqlServerRepository<Invoice> _invoiceRepository = invoiceRepository;
        private readonly SqlServerRepository<InvoiceItems> _invoiceItemsRepository = invoiceItemsRepository;
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork = sqlServerUnitOfWork;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork = sqliteUnitOfWork;
        private readonly IFiscalRepository _fiscalRepository = fiscalRepository;

        /// <summary>
        /// Decrypts a list of incoming <see cref="FileRecordDTO"/> objects,
        /// extracts invoice data, and saves each valid invoice (with items) to the database.
        /// </summary>
        /// <param name="dtos">List of encrypted file record DTOs containing invoice data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{FileRecordDTO}"/> indicating success or failure of the save operation.
        /// </returns>
        public async Task<ApiResponse<FileRecordDTO>> DecryptAndSaveInvoicesAsync(List<FileRecordDTO> dtos)
        {
            // 🟢 Guard-clause: no input
            if (dtos == null || dtos.Count == 0)
                return new ApiResponse<FileRecordDTO>(
                    ApiStatusCode.Error.ToString(),
                    ResponseMessages.DataNotFound,
                    null);

            try
            {
                foreach (var item in dtos)
                {
                    //   Decrypt the payload
                    var decrypted = ModernAESEncryption.Decrypt(item.InvoiceData!, _settings.EC);
                    if (string.IsNullOrWhiteSpace(decrypted))
                        continue;

                    //   Get the JSON part
                    var jsonPart = decrypted.Split('|')[0];
                    if (string.IsNullOrWhiteSpace(jsonPart))
                        continue;

                    //   Deserialize to DTO
                    var invoiceDto = JsonSerializer.Deserialize<InvoiceDto>(
                        jsonPart,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (invoiceDto == null)
                        continue;

                    //   Save invoice & items
                    var response = await CreateInvoiceWithItemsAsync(invoiceDto);
                    if (string.Equals(response.StatusCode,
                                      ApiStatusCode.Success.ToString(),
                                      StringComparison.OrdinalIgnoreCase))
                    {
                        return new ApiResponse<FileRecordDTO>(
                            ApiStatusCode.Success.ToString(),
                            ResponseMessages.RecordSaved,
                            null);
                    }
                }

                // No invoice succeeded
                return new ApiResponse<FileRecordDTO>(
                    ApiStatusCode.Error.ToString(),
                    ResponseMessages.UnknownError,
                    null);
            }
            catch (Exception ex)
            {
                return new ApiResponse<FileRecordDTO>(
                    ApiStatusCode.Error.ToString(),
                    ex.Message,
                    null);
            }
        }

        /// <summary>
        /// Creates a single invoice record along with any associated
        /// invoice items, saving both to the SQL Server database.
        /// </summary>
        /// <param name="dto">The invoice data transfer object to persist.</param>
        /// <returns>
        /// An <see cref="ApiResponse{Invoice}"/> indicating success or failure,
        /// including the saved <see cref="Invoice"/> entity when successful.
        /// </returns>
        private async Task<ApiResponse<Invoice>> CreateInvoiceWithItemsAsync(InvoiceDto dto)
        {
            try
            {
                // Map & save invoice
                var invoice = _mapper.Map<Invoice>(dto);
                await _invoiceRepository.AddAsync(invoice);
                await _sqlServerUnitOfWork.SaveChangesAsync();

                // Map & save items (if any)
                if (dto.InvoiceItemDto?.Count > 0)
                {
                    var items = _mapper.Map<List<InvoiceItems>>(dto.InvoiceItemDto);
                    // Assign the generated InvoiceID to each item
                    items.ForEach(i => i.InvoiceID = invoice.InvoiceID);

                    await _invoiceItemsRepository.AddRangeAsync(items);
                    await _sqlServerUnitOfWork.SaveChangesAsync();
                }

                return new ApiResponse<Invoice>(
                    ApiStatusCode.Success.ToString(),
                    ResponseMessages.RecordSaved,
                    invoice);
            }
            catch (Exception ex)
            {
                return new ApiResponse<Invoice>(
                    statusCode: ApiStatusCode.Error.ToString(),
                    message: ex.Message,
                    data: null
                );
            }
        }

        /// <summary>
        /// Retrieves filtered invoices and converts them into a CSV-formatted string.
        /// </summary>
        /// <param name="dto">Filter criteria such as POS ID and date range.</param>
        /// <returns>
        /// An <see cref="ApiResponse{String}"/> containing the CSV representation of the invoices.
        /// </returns>
        public async Task<ApiResponse<string>> GetInvoicesCsvAsync(InvoiceFilterDto dto)
        {
            // Get the filtered invoices
            var invoices = await GetInvoicesAsync(dto);

            // Convert to CSV
            var csv = CsvUtility.ToCsv(invoices);

            // Wrap in your ApiResponse<T>
            return new ApiResponse<string>(
                ApiStatusCode.Success.ToString(),
                ResponseMessages.RecordFound,
                csv
            );
        }

        /// <summary>
        /// Applies filters to retrieve invoices from the SQL Server repository.
        /// </summary>
        /// <param name="dto">Filter object containing POS ID and optional date range.</param>
        /// <returns>A filtered collection of <see cref="Invoice"/> entities.</returns>
        private async Task<IEnumerable<Invoice>> GetInvoicesAsync(InvoiceFilterDto dto)
        {
            try
            {
                IQueryable<Invoice> query = _invoiceRepository.Query();

                // Always restrict by the current POS (logged-in user)
                query = query.Where(i => i.POSID == dto.PosId);

                if (dto.FromDate.HasValue)
                    query = query.Where(i => i.EntryDate >= dto.FromDate.Value);

                if (dto.ToDate.HasValue)
                    query = query.Where(i => i.EntryDate <= dto.ToDate.Value);

                return await query.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}