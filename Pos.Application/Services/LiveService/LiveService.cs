using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.Interfaces;
using Pos.Application.Utility;
using Pos.Domain.Entities;
using System.Text.Json;

namespace Pos.Application.Services.LiveService
{
    /// <summary>
    /// Provides live operations such as decrypting incoming invoice data,
    /// saving invoices and their items to SQL Server, and exporting
    /// filtered invoices as CSV.
    /// </summary>
    public class LiveService(
        IOptions<AppSettings> options,
        ISqlServerRepositoryFactory sqlRepositoryFactory,
        IMapper mapper,
        ISqlServerUnitOfWork sqlServerUnitOfWork) : ILiveService
    {
        private readonly AppSettings _settings = options.Value;
        private readonly IRepository<Invoice> _sqlInvoiceRepository = sqlRepositoryFactory.CreateRepository<Invoice>();
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork = sqlServerUnitOfWork;

        /// <summary>
        /// Decrypts a list of incoming <see cref="FileRecordDTO"/> objects,
        /// extracts invoice data, and saves each valid invoice (with items) to the database.
        /// </summary>
        /// <param name="dtos">List of encrypted file record DTOs containing invoice data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{FileRecordDTO}"/> indicating success or failure of the save operation.
        /// </returns>
        public async Task<ApiResponse<List<FileRecordDto>>> DecryptAndSaveInvoicesAsync(List<FileRecordDto> dtos)
        {
            // Guard-clause: no input
            if (dtos == null || dtos.Count == 0)
            {
                return new ApiResponse<List<FileRecordDto>>(
                    ApiStatusCode.Error.ToString(),
                    ResponseMessages.DataNotFound,
                    null);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var anySaved = false;
                var syncedRecords = new List<FileRecordDto>();

                foreach (var item in dtos)
                {
                    if (string.IsNullOrEmpty(item.InvoiceNumber)) continue;
                    // Decrypt
                    var decrypted = ModernAESEncryption.Decrypt(item.InvoiceData!, _settings.EC);
                    if (string.IsNullOrWhiteSpace(decrypted)) continue;

                    // Extract JSON
                    var jsonPart = decrypted.Split('|')[0];
                    if (string.IsNullOrWhiteSpace(jsonPart)) continue;

                    jsonPart = jsonPart.Replace("FBRInvoiceNumber", "InvoiceNumber");
                    // Deserialize
                    if (JsonSerializer.Deserialize<InvoiceDto>(jsonPart, options) is not { } invoiceDto) continue;

                    // Save invoice & items
                    var response = await CreateInvoiceWithItemsAsync(invoiceDto);
                    if (string.Equals(response.StatusCode, ApiStatusCode.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        anySaved = true;
                        item.IsSynced = (int)InvoiceStatus.Synced;
                        syncedRecords.Add(item);
                    }
                }

                return anySaved
                    ? new ApiResponse<List<FileRecordDto>>(
                        ApiStatusCode.Success.ToString(),
                        ResponseMessages.RecordSaved,
                        syncedRecords, null) // or pass syncedRecords if you want to return them
                    : new ApiResponse<List<FileRecordDto>>(
                        ApiStatusCode.Error.ToString(),
                        ResponseMessages.UnknownError,
                        null);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<FileRecordDto>>(
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
        public async Task<ApiResponse<Invoice>> CreateInvoiceWithItemsAsync(InvoiceDto dto)
        {
            try
            {
                // Map & save invoice
                var invoice = _mapper.Map<Invoice>(dto);
                invoice.EntryDate = DateTime.Now;
                invoice.FBRInvoiceNumber = dto.InvoiceNumber;
                //invoice.FBRInvoiceNumber = GlobalMethods.InvoiceNumber(dto.POSID);
                await _sqlInvoiceRepository.AddAsync(invoice);
                await _sqlServerUnitOfWork.SaveChangesAsync();

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

            if (!invoices.Any())
            {
                return new ApiResponse<string>(
                    ApiStatusCode.Success.ToString(),
                    ResponseMessages.DataNotFound,
                    null
                );
            }

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
                IQueryable<Invoice> query = _sqlInvoiceRepository.Query();

                // Only filter POS if given
                if (dto.PosId > 0 && dto.PosId is not null)
                    query = query.Where(i => i.POSID == dto.PosId);

                if (dto.FromDate.HasValue)
                    query = query.Where(i => i.EntryDate.Date >= dto.FromDate.Value.Date);

                if (dto.ToDate.HasValue)
                    query = query.Where(i => i.EntryDate.Date <= dto.ToDate.Value.Date);

                return await query.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

    }
}