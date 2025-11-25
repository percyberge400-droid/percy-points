using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
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
        public LiveService(
            IConfiguration configuration,
            IInvoiceRepository invoiceRepository,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            IOptions<AppSettings> options
            ,

            IPosClientRepository posClientRepository
            )
        {
            // Read EC key from multiple sources
            //if (!string.IsNullOrWhiteSpace(configuration["AppSettings:EC"]))
            //{
            //    _ec = configuration["AppSettings:EC"];
            //}
            //else if (!string.IsNullOrWhiteSpace(configuration["EC"]))
            //{
            //    _ec = configuration["EC"];
            //}
            _appSettings = options.Value; // CHANGED


            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sqlServerUnitOfWork = sqlServerUnitOfWork ?? throw new ArgumentNullException(nameof(sqlServerUnitOfWork));
            _posClientRepository = posClientRepository;
        }

        public async Task<ApiResponse<List<FileRecordDto>>> DecryptAndSaveInvoicesAsync(List<FileRecordDto> dtos, string environment)
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
                    if (string.IsNullOrEmpty(item.InvoiceNumber)) continue;

                    string decrypted = "";
                    try
                    {
                        decrypted = ModernAESEncryption.Decrypt(item.InvoiceData!, _appSettings.EC);
                        if (string.IsNullOrWhiteSpace(decrypted))
                        {
                            decrypted = ModernAESEncryption.Decrypt(item.InvoiceData!, posClient.E_Key);
                            if (string.IsNullOrWhiteSpace(decrypted)) { continue; }
                        }
                    }
                    catch
                    {
                        decrypted = AESEncryption.Decrypt(item.InvoiceData!, posClient.E_Key);
                        if (string.IsNullOrWhiteSpace(decrypted))
                        {
                            decrypted = AESEncryption.Decrypt(item.InvoiceData!, _appSettings.EC);
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
                return new ApiResponse<List<FileRecordDto>>(
                    ApiStatusCode.Error.ToString(),
                    ex.Message,
                    null);
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

                await _invoiceRepository.AddAsync(invoice, environment);

                return new ApiResponse<Invoice>(
                    ApiStatusCode.Success.ToString(),
                    ResponseMessages.RecordSaved,
                    invoice);
            }
            catch (Exception ex)
            {
                return new ApiResponse<Invoice>(
                    ApiStatusCode.Error.ToString(),
                    ex.Message,
                    null);
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
            try
            {
                return await _invoiceRepository.GetInvoicesAsync(dto, env);
            }
            catch
            {
                throw;
            }
        }
    }
}
