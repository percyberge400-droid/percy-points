using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;
using Pos.Application.Utility.OldDecryption;
using Pos.Domain.Entities;

namespace Pos.Application.Services.LiveService
{
    /// <summary>
    /// Provides live operations such as decrypting incoming invoice data,
    /// saving invoices and their items to SQL Server, and exporting
    /// filtered invoices as CSV.
    /// </summary>
    public class LiveService : ILiveService
    {
        private readonly string _ec;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPosClientRepository _posClientRepository;
        private readonly IMapper _mapper;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly AppSettings _appSettings;
        private readonly AESEncryption _aESEncryption;
        private readonly ICloudLogService _cloudLogService;
        public LiveService(
            IConfiguration configuration,
            IInvoiceRepository invoiceRepository,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            IOptions<AppSettings> options
            ,

            IPosClientRepository posClientRepository
,
            AESEncryption aESEncryption,
            ICloudLogService cloudLogService)
        {
            _appSettings = options.Value;
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sqlServerUnitOfWork = sqlServerUnitOfWork ?? throw new ArgumentNullException(nameof(sqlServerUnitOfWork));
            _posClientRepository = posClientRepository;
            _aESEncryption = aESEncryption;
            _cloudLogService = cloudLogService;
        }

        public async Task<ApiResponse<List<FileRecordDto>>> DecryptAndSaveInvoicesAsync(List<FileRecordDto> dtos, string environment, bool isWindows7 = false)
        {
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

                var posClient = await _posClientRepository.GetByPosId(dtos[0].POSID, environment);

                foreach (var item in dtos)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(item.InvoiceNumber)) continue;
                        string decrypted = "";
                        try
                        {
                            if (isWindows7)
                            {
                                decrypted = await _aESEncryption.DecryptWindows7Async(item.InvoiceData!, _appSettings.EC!);
                            }
                            else
                            {
                                decrypted = await _aESEncryption.DecryptAsync(item.InvoiceData!, posClient.E_Key!);
                                if (string.IsNullOrWhiteSpace(decrypted))
                                {
                                    decrypted = await _aESEncryption.DecryptAsync(item.InvoiceData!, _appSettings.EC!);
                                    if (string.IsNullOrEmpty(decrypted))
                                    {
                                        decrypted = OldAESEncryption.Decrypt(item.InvoiceData!, posClient.E_Key!);
                                        if (string.IsNullOrWhiteSpace(decrypted))
                                        {
                                            decrypted = OldAESEncryption.Decrypt(item.InvoiceData!, _appSettings.EC);
                                            if (string.IsNullOrWhiteSpace(decrypted)) { continue; }
                                        }
                                    }
                                    if (string.IsNullOrWhiteSpace(decrypted)) { continue; }
                                }
                            }
                        }
                        catch
                        {
                            decrypted = OldAESEncryption.Decrypt(item.InvoiceData!, posClient.E_Key!);
                            if (string.IsNullOrWhiteSpace(decrypted))
                            {
                                decrypted = OldAESEncryption.Decrypt(item.InvoiceData!, _appSettings.EC);
                                if (string.IsNullOrWhiteSpace(decrypted)) { continue; }
                            }
                        }

                        var jsonPart = decrypted.Split('|')[0];
                        if (string.IsNullOrWhiteSpace(jsonPart)) continue;

                        jsonPart = jsonPart.Replace("FBRInvoiceNumber", "InvoiceNumber");

                        if (JsonSerializer.Deserialize<InvoiceDto>(jsonPart, options) is not { } invoiceDto) continue;

                        var response = await CreateInvoiceWithItemsAsync(invoiceDto, environment);
                        if (string.Equals(response.StatusCode, ApiStatusCode.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            anySaved = true;
                            item.IsSynced = (int)InvoiceStatus.Synced;
                            syncedRecords.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Build and log exception
                        var dto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                            .WithExceptionInfo(ex)
                            .WithDomainInfo("Live SErvice", "DecryptAndSaveInvoicesAsync In Foreach Loop", null); // adjust module/action as needed

                        await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { dto }, environment);
                    }
                }

                return anySaved
                    ? new ApiResponse<List<FileRecordDto>>(
                        ApiStatusCode.Success.ToString(),
                        ResponseMessages.RecordSaved,
                        syncedRecords)
                    : new ApiResponse<List<FileRecordDto>>(
                        ApiStatusCode.Error.ToString(),
                        ResponseMessages.UnknownError,
                        null);
            }
            catch (Exception ex)
            {
                // Build and log exception
                var dto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                    .WithExceptionInfo(ex)
                    .WithDomainInfo("Live SErvice", "DecryptAndSaveInvoicesAsync out side the loop", null); // adjust module/action as needed

                await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { dto }, environment);

                // Return API response
                return new ApiResponse<List<FileRecordDto>>(
                    ApiStatusCode.Error.ToString(),
                    ex.Message,
                    null
                );
            }
        }

        public async Task<ApiResponse<Invoice>> CreateInvoiceWithItemsAsync(InvoiceDto dto, string environment)
        {
            try
            {
                var invoice = _mapper.Map<Invoice>(dto);
                invoice.EntryDate = DateTime.Now;
                invoice.FBRInvoiceNumber = dto.InvoiceNumber;
                invoice.BuyerNTN = dto.BuyerPNTN;
                invoice.BuyerCNIC = dto.BuyerCNIC?.Replace("-", "");

                if (!string.IsNullOrEmpty(invoice.BuyerCNIC) && invoice.BuyerCNIC.Length > 13)
                {
                    invoice.BuyerCNIC = invoice.BuyerCNIC.Substring(0, 13);
                }

                await _invoiceRepository.AddAsync(invoice, environment);

                return new ApiResponse<Invoice>(
                    ApiStatusCode.Success.ToString(),
                    ResponseMessages.RecordSaved,
                    invoice);
            }
            catch (Exception ex)
            {
                // Build and log exception
                var logDto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                    .WithExceptionInfo(ex)
                    .WithDomainInfo("InvoiceService", "YourActionName", null); // replace module/action as needed

                await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { logDto }, environment);

                // Return API response with error
                return new ApiResponse<Invoice>(
                    ApiStatusCode.Error.ToString(),
                    ex.Message,
                    null
                );
            }

        }

        public async Task<ApiResponse<string>> GetInvoicesCsvAsync(InvoiceFilterDto dto, string environment)
        {
            var invoices = await GetInvoicesAsync(dto, environment);

            if (!invoices.Any())
            {
                return new ApiResponse<string>(
                    ApiStatusCode.Success.ToString(),
                    ResponseMessages.DataNotFound,
                    null);
            }

            var csv = CsvUtility.ToCsv(invoices);

            return new ApiResponse<string>(
                ApiStatusCode.Success.ToString(),
                ResponseMessages.RecordFound,
                csv);
        }

        private async Task<IEnumerable<Invoice>> GetInvoicesAsync(InvoiceFilterDto dto, string env)
        {
            return await _invoiceRepository.GetInvoicesAsync(dto, env);
        }
    }
}
