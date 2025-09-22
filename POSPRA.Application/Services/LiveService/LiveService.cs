using System.Text.Json;
using AutoMapper;
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
    public class LiveService : ILiveService
    {
        private readonly AppSettings _settings;
        private readonly SqlServerRepository<Invoice> _invoiceRepository;
        private readonly SqlServerRepository<InvoiceItems> _invoiceItemsRepository;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IFiscalRepository _fiscalRepository;

        public LiveService(
            IOptions<AppSettings> options,
            SqlServerRepository<Invoice> invoiceRepository,
            SqlServerRepository<InvoiceItems> invoiceItemsRepository,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IFiscalRepository fiscalRepository)
        {
            _settings = options.Value;          // ← this should have EC populated
            _invoiceRepository = invoiceRepository;
            _invoiceItemsRepository = invoiceItemsRepository;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _fiscalRepository = fiscalRepository;
        }


        public async Task<ApiResponse<FileRecordDTO>> DecryptAndSaveInvoicesAsync(List<FileRecordDTO> dtos)
        {
            // 🟢 Guard-clause: no input
            if (dtos == null || dtos.Count == 0)
            {
                return new ApiResponse<FileRecordDTO>(
                    ApiStatusCode.Error.ToString(),
                    ResponseMessages.DataNotFound,
                    null);
            }

            try
            {
                foreach (var item in dtos)
                {
                    // 1️⃣  Decrypt the payload
                    var decrypted = ModernAESEncryption.Decrypt(item.InvoiceData!, _settings.EC);
                    if (string.IsNullOrWhiteSpace(decrypted))
                        continue;

                    // 2️⃣  Get the JSON part
                    var jsonPart = decrypted.Split('|')[0];
                    if (string.IsNullOrWhiteSpace(jsonPart))
                        continue;

                    // 3️⃣  Deserialize to DTO
                    var invoiceDto = JsonSerializer.Deserialize<InvoiceDto>(
                        jsonPart,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (invoiceDto == null)
                        continue;

                    // 4️⃣  Save invoice & items
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
    }
}
