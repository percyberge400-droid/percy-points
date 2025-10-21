using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.FileRecordRepository;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace POSPRA.Application.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        public readonly IMapper _mapper;
        private readonly IFileRecordRepository _fileRecordRepository;
        private readonly AppSettings _settings;
        private readonly InvoiceValidatorService _invoiceValidatorService;
        private readonly INetworkService _networkService;
        private readonly ILiveService _liveService;
        private readonly IFileRecordService _fileRecordService;
        private readonly ILogService _logService;
        private readonly SqlServerRepository<Invoice> _invoiceRepository;

        public InvoiceService(IMapper mapper,
            IOptions<AppSettings> options,
            InvoiceValidatorService invoiceValidatorService,
            INetworkService networkService,
            ILiveService liveService,
            IFileRecordService fileRecordService,
            ILogService logService,
            IFileRecordRepository fileRecordRepository,
            SqlServerRepository<Invoice> invoiceRepository)
        {
            _mapper = mapper;
            _settings = options.Value;
            _invoiceValidatorService = invoiceValidatorService;
            _networkService = networkService;
            _liveService = liveService;
            _fileRecordService = fileRecordService;
            _logService = logService;
            _fileRecordRepository = fileRecordRepository;
            _invoiceRepository = invoiceRepository;
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
                var output = await _fileRecordRepository.FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber);
                if (output != null)
                {
                    var decrypted = ModernAESEncryption.Decrypt(output.InvoiceData!, _settings.EC);
                    var jsonPart = decrypted.Split('|')[0];
                    if (string.IsNullOrWhiteSpace(jsonPart) ||
                        JsonSerializer.Deserialize<InvoiceDto>(jsonPart, options) is not { } invoiceDto)
                        return new ApiResponse<InvoiceDto>(
                            statusCode: ApiStatusCode.Error,
                            message: ResponseMessages.DataNotFound,
                            data: null!
                        );

                    return new ApiResponse<InvoiceDto>(
                        statusCode: ApiStatusCode.Success,
                        message: ResponseMessages.RecordFound,
                        data: invoiceDto
                    );
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
        public async Task<ApiResponse<InvoiceDto>> CreateAsync(InvoiceDto dto)
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

                bool isValid = validation.IsValid;
                if (!isValid)
                {
                    // ✅ log but don't exit
                    await LogError(validation.ErrorMessages);
                }

                // ✅ 2. Create fiscal invoice (runs regardless of validation)
                var fiscalResponse = await GenerateInvoicePackageAsync(invoiceEntity);
                if (fiscalResponse.StatusCode != ApiStatusCode.Success)
                {
                    await LogError($"Invoice not available for {dto.InvoiceType}");
                    return ErrorResponse(ResponseMessages.UnknownError);
                }

                // ✅ 3. Try to sync with live if internet is available
                dto.FBRInvoiceNumber = invoiceEntity.FBRInvoiceNumber;
                if (await _networkService.IsInternetAvailableAsync())
                {
                    if (dto.FBRInvoiceNumber != null)
                    {
                        var isInvoiceExist = await isCloudInvoiceExists(invoiceEntity.FBRInvoiceNumber);
                        if (!isInvoiceExist)
                        {
                            var liveResponse = await _liveService.CreateInvoiceWithItemsAsync(dto);
                            if (liveResponse.StatusCode == ApiStatusCode.Success)
                            {
                                var record = await _fileRecordService.GetByInvoiceIdAsync(fiscalResponse.Data.InvoiceId);
                                if (record.StatusCode == ApiStatusCode.Success)
                                {
                                    record.Data.IsSynced = (int)InvoiceStatus.Synced;

                                    await _fileRecordService.UpdateFileRecordAsync(record.Data);
                                }
                            }
                        }
                    }
                }

                // ✅ 4. Return success if fiscal creation worked, but include validation info
                return new ApiResponse<InvoiceDto>(
                    isValid ? ApiStatusCode.Success : ApiStatusCode.Error,
                    isValid ? ResponseMessages.RecordSaved : "Invoice saved but failed validation",
                    null,
                    isValid ? string.Empty : string.Join(" | ", string.Empty)
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
        private async Task<bool> isCloudInvoiceExists(string invoiceNumber)
        {
            return await _invoiceRepository.ExistsAsync(x => x.FBRInvoiceNumber == invoiceNumber);
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
        private async Task<ApiResponse<(string EncryptedPackage, int InvoiceId)>> GenerateInvoicePackageAsync(Invoice invoice)
        {
            try
            {
                //var posId = _requestHeaderService.GetPosId();
                // 1️ Generate invoice number
                string invoiceNumber = GlobalMethods.InvoiceNumber(_settings.POS);
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
                var encryptedData = ModernAESEncryption.Encrypt(textToEncrypt, aesKey);

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
                await _logService.CreateLogAsync(new Logs(errorMessage, AlertType.Exception, false));
                return new ApiResponse<(string, int)>(
                ApiStatusCode.Error,
                ResponseMessages.UnknownError,
                (string.Empty, 0),
                string.Empty);
            }
        }
    }
}