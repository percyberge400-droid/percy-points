using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pos.Application.AutoMapperProfile;
using Pos.Application.DTOs;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.InvoiceDTOs;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.FileRecordService;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;
using Pos.Application.Services.NetworkService;
using Pos.Application.Utility;
using Pos.Application.Utility.OldDecryption;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using POSPRA.Application.Services.FiscalService;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Pos.Application.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        public readonly IMapper _mapper;
        private readonly IRepository<FileRecord> _sqlFileRecordRepository;
        private readonly IRepository<Invoice> _sqlinvoiceRepository;


        private readonly AppSettings _settings;
        private readonly InvoiceValidatorService _invoiceValidatorService;
        private readonly INetworkService _networkService;
        private readonly ILiveService _liveService;
        private readonly IFileRecordService _fileRecordService;
        private readonly ILogService _logService;
        private readonly AESEncryption _aESEncryption;
        public InvoiceService(IMapper mapper,
            ISqliteRepositoryFactory sqliteRepositoryFactory,
            ISqlServerRepositoryFactory sqlServerRepositoryFactory,
            IOptions<AppSettings> options,
            InvoiceValidatorService invoiceValidatorService,
            INetworkService networkService,
            ILiveService liveService,
            IFileRecordService fileRecordService,
            ILogService logService,
            AESEncryption aESEncryption)
        {
            _sqlFileRecordRepository = sqliteRepositoryFactory.CreateRepository<FileRecord>();
            _sqlinvoiceRepository = sqlServerRepositoryFactory.CreateRepository<Invoice>();
            _mapper = mapper;
            _settings = options.Value;
            _invoiceValidatorService = invoiceValidatorService;
            _networkService = networkService;
            _liveService = liveService;
            _fileRecordService = fileRecordService;
            _logService = logService;
            _aESEncryption = aESEncryption;
        }

        public async Task<ApiResponse<InvoiceDto>> GetInvoiceWithItems(string invoiceNumber)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            try
            {
                var output = await _sqlFileRecordRepository.FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber);
                if (output != null)
                {
                    try
                    {
                        var decrypted = await _aESEncryption.DecryptAsync(output.InvoiceData!, _settings.EC);
                        var jsonPart = decrypted.Split('|')[0];
                        if (string.IsNullOrWhiteSpace(jsonPart) ||
                            JsonSerializer.Deserialize<InvoiceDto>(jsonPart, options) is not { } invoiceDto)
                            return new ApiResponse<InvoiceDto>(
                                statusCode: ApiStatusCode.Error,
                                message: ResponseMessages.DataNotFound,
                                data: null!
                            );
                        var buyerNtn = JsonDocument.Parse(jsonPart)
                            .RootElement.GetProperty("BuyerNTN")
                            .GetString();

                        invoiceDto.BuyerPNTN = buyerNtn;

                        return new ApiResponse<InvoiceDto>(
                            statusCode: ApiStatusCode.Success,
                            message: ResponseMessages.RecordFound,
                            data: invoiceDto
                        );
                    }
                    catch
                    {
                        var decrypted = OldAESEncryption.Decrypt(output.InvoiceData!, _settings.EC);
                        var jsonPart = decrypted.Split('|')[0];

                        if (string.IsNullOrWhiteSpace(jsonPart))
                        {
                            return new ApiResponse<InvoiceDto>(
                                statusCode: ApiStatusCode.Error,
                                message: ResponseMessages.DataNotFound,
                                data: null!
                            );
                        }

                        // Deserialize Old DTO
                        var oldDto = JsonSerializer.Deserialize<OldInvoiceDto>(jsonPart, options);

                        if (oldDto is null)
                        {
                            return new ApiResponse<InvoiceDto>(
                                statusCode: ApiStatusCode.Error,
                                message: ResponseMessages.DataNotFound,
                                data: null!
                            );
                        }

                        // Map → New DTO
                        var invoiceDto = oldDto.ToNewInvoiceDto();

                        return new ApiResponse<InvoiceDto>(
                            statusCode: ApiStatusCode.Success,
                            message: ResponseMessages.RecordFound,
                            data: invoiceDto
                        );

                    }

                }

                return new ApiResponse<InvoiceDto>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.DataNotFound,
                    data: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<InvoiceDto>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.UnknownError,
                    data: null!
                );
            }
        }

        /// <summary>
        /// Creates a new invoice by validating it, generating a fiscal invoice,
        /// and logging any errors or exceptions. Returns an ApiResponse containing
        /// the status and any relevant messages.
        /// </summary>
        /// <param name="dto">The invoice object to create.</param>
        /// <returns>An ApiResponse containing the result of the operation.</returns>
        public async Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto, string environment)
        {
            try
            {
                if (dto == null)
                {
                    await LogError("Invalid model");
                    return ErrorResponse(ResponseMessages.DataNotFound);
                }

                // Map & validate entity
                var invoiceEntity = _mapper.Map<Invoice>(dto);
                var validation = _invoiceValidatorService.ValidateInvoice(invoiceEntity);

                if (!validation.IsValid)
                {
                    await LogError($"Invoice validation failed: {validation.ErrorMessages}");
                    return ErrorResponse("Invoice validation failed", validation.ErrorMessages);
                }

                if (String.IsNullOrEmpty(environment))
                    environment = _settings.Environment;

                // ✅ 2. Create fiscal invoice (runs regardless of validation)
                var fiscalResponse = await GenerateInvoicePackageAsync(invoiceEntity, environment);
                if (fiscalResponse.StatusCode != ApiStatusCode.Success)
                {
                    await LogError($"Invoice not available for {dto.InvoiceType}");
                    return ErrorResponse(ResponseMessages.UnknownError);
                }

                // ✅ 3. Try to sync with live if internet is available
                //dto.InvoiceNumber = invoiceEntity.FBRInvoiceNumber;
                //if (await _networkService.IsInternetAvailableAsync())
                //{
                //    if (dto.InvoiceNumber != null)
                //    {
                //        var isInvoiceExist = await isCloudInvoiceExists(invoiceEntity.FBRInvoiceNumber);
                //        if (!isInvoiceExist)
                //        {
                //            var liveResponse = await _liveService.CreateInvoiceWithItemsAsync(dto);
                //            if (liveResponse.StatusCode == ApiStatusCode.Success)
                //            {
                //                var record = await _fileRecordService.GetByInvoiceIdAsync(fiscalResponse.Data.InvoiceId);
                //                if (record.StatusCode == ApiStatusCode.Success)
                //                {
                //                    record.Data.IsSynced = (int)InvoiceStatus.Synced;

                //                    await _fileRecordService.UpdateFileRecordAsync(record.Data);
                //                }
                //            }
                //        }
                //    }
                //}

                // 4. Return success if fiscal creation worked, but include validation info
                return new ApiResponse<InvoiceDto>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordSaved,
                    dto,
                    string.Empty
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(ResponseMessages.UnknownError, ex.InnerException?.Message ?? ex.Message);
            }

            // ----- Local helpers -----
            async Task LogError(string message) =>
                await _logService.CreateLogAsync(
                    _logService.BuildLog(message, AlertType.Exception, "Invoice", nameof(CreateAsync)));

            ApiResponse<InvoiceDto> ErrorResponse(string msg, string err = "") =>
                new(ApiStatusCode.Error, msg, null, err);
        }

        public async Task<InvoiceResponseDto> OldCreateAsync(InvoiceDto dto, string environment)
        {
            try
            {
                if (dto == null)
                {
                    await LogError("Invalid model");
                    return new InvoiceResponseDto
                    {
                        InvoiceNumber = "Not Available",
                        Code = "402",
                        Response = "Fiscal invoice creation failed.",
                        Errors = "Invoice not created"
                    };
                }

                // Map & validate entity
                var invoiceEntity = _mapper.Map<Invoice>(dto);
                var validation = _invoiceValidatorService.ValidateInvoice(invoiceEntity);

                if (!validation.IsValid)
                {
                    await LogError($"Invoice validation failed: {validation.ErrorMessages}");
                    return new InvoiceResponseDto
                    {
                        InvoiceNumber = "Not Available",
                        Code = "402",
                        Response = "Fiscal invoice creation failed.",
                        Errors = $"Invoice validation failed: {validation.ErrorMessages}"
                    };
                }

                if (String.IsNullOrEmpty(environment))
                    environment = _settings.Environment;

                // ✅ 2. Create fiscal invoice (runs regardless of validation)
                var fiscalResponse = await GenerateInvoicePackageAsync(invoiceEntity, environment);
                if (fiscalResponse.StatusCode != ApiStatusCode.Success)
                {
                    await LogError($"Invoice not available for {dto.InvoiceType}");
                    return new InvoiceResponseDto
                    {
                        InvoiceNumber = "Not Available",
                        Code = "402",
                        Response = "Fiscal invoice creation failed.",
                        Errors = "Invoice not created"
                    };
                }

                // ✅ 3. Try to sync with live if internet is available
                //dto.InvoiceNumber = invoiceEntity.FBRInvoiceNumber;
                //if (await _networkService.IsInternetAvailableAsync())
                //{
                //    if (dto.InvoiceNumber != null)
                //    {
                //        var isInvoiceExist = await isCloudInvoiceExists(invoiceEntity.FBRInvoiceNumber);
                //        if (!isInvoiceExist)
                //        {
                //            var liveResponse = await _liveService.CreateInvoiceWithItemsAsync(dto);
                //            if (liveResponse.StatusCode == ApiStatusCode.Success)
                //            {
                //                var record = await _fileRecordService.GetByInvoiceIdAsync(fiscalResponse.Data.InvoiceId);
                //                if (record.StatusCode == ApiStatusCode.Success)
                //                {
                //                    record.Data.IsSynced = (int)InvoiceStatus.Synced;

                //                    await _fileRecordService.UpdateFileRecordAsync(record.Data);
                //                }
                //            }
                //        }
                //    }
                //}

                // 4. Return success if fiscal creation worked, but include validation info
                return new InvoiceResponseDto
                {
                    InvoiceNumber = invoiceEntity.FBRInvoiceNumber,
                    Code = "100",
                    Response = "Fiscal Invoice Number generated successfully.",
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                return new InvoiceResponseDto
                {
                    InvoiceNumber = "Not Available",
                    Code = "402",
                    Response = "Fiscal invoice creation failed.",
                    Errors = ResponseMessages.UnknownError 
                };
            }

            // ----- Local helpers -----
            async Task LogError(string message) =>
                await _logService.CreateLogAsync(
                    _logService.BuildLog(message, AlertType.Exception, "Invoice", nameof(CreateAsync)));
        }

        private async Task<bool> isCloudInvoiceExists(string invoiceNumber)
        {
            return await _sqlinvoiceRepository.ExistsAsync(x => x.FBRInvoiceNumber == invoiceNumber);
        }
        /// <summary>
        /// Generates a fiscal invoice by serializing, signing, and encrypting
        /// the invoice data. It then inserts the encrypted invoice into the database.
        /// </summary>
        /// <param name="invoice">The invoice to process.</param>
        /// <returns>
        /// A string containing the encrypted invoice package if successful;
        /// otherwise, an empty string.
        /// </returns>
        private async Task<ApiResponse<(string EncryptedPackage, int InvoiceId)>> GenerateInvoicePackageAsync(Invoice invoice, string? environment)
        {
            try
            {
                //var posId = _requestHeaderService.GetPosId();
                // 1️ Generate invoice number
                string invoiceNumber = GlobalMethods.InvoiceNumber(_settings.POS, environment);
                invoice.FBRInvoiceNumber = invoiceNumber;

                // 2️ Serialize invoice
                string invoiceData = JsonConvert.SerializeObject(invoice);
                bool isValidCalculation = false;

                // 3️ Create payload
                string payload = $"{invoiceData}|{isValidCalculation}|Latest";

                // 4️ Sign the payload using RSA
                // Replace GenerateKeys() with actual PEM private key for production
                var (privateKey, publicKeyPem) = DataSigning.GenerateKeys();
                string signature = DataSigning.Sign(privateKey, payload);

                // 5️ Optional: verify signature immediately
                bool verified = DataSigning.Verify(publicKeyPem, payload, signature);

                // 6️ Generate 256-bit AES key from _settings.EC
                byte[] aesKey = Encoding.UTF8.GetBytes(_settings.EC.PadRight(32).Substring(0, 32));

                // 7️ Encrypt payload + signature using AES-GCM
                string textToEncrypt = $"{payload}|{signature}";
                var encryptedData = _aESEncryption.Encrypt(textToEncrypt, aesKey);

                // 8️ Combine into final encrypted package
                string encryptedPackage = $"{encryptedData.cipherText}:{encryptedData.nonce}:{encryptedData.tag}";

                // 9️ Insert invoice and return invoice number
                int invoiceId = await _fileRecordService.CreateAsync(invoice.POSID, encryptedPackage, invoiceNumber);

                // You can return encryptedPackage if needed for fiscal system
                return new ApiResponse<(string, int)>(
                ApiStatusCode.Success,
                ResponseMessages.RecordSaved,
                (encryptedPackage, invoiceId),
                string.Empty);
            }
            catch (Exception ex)
            {
                string errorMessage = $"{GlobalVariables.DATE} CreateFiscalInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";
                await _logService.CreateLogAsync(new CreateLogDto(errorMessage, AlertType.Exception, false));
                return new ApiResponse<(string, int)>(
                ApiStatusCode.Error,
                ResponseMessages.UnknownError,
                (string.Empty, 0),
                string.Empty);
            }
        }
    }
}