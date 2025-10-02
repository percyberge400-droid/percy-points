using System.Text;
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
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.Application.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        private readonly InvoiceValidatorService _invoiceValidatorService;
        private readonly IMapper _mapper;
        private readonly INetworkService _networkService;
        private readonly ILogService _logService;
        private readonly ILiveService _liveService;
        private readonly IFileRecordService _fileRecordService;
        private readonly AppSettings _settings;

        public InvoiceService(InvoiceValidatorService invoiceValidatorService,
            IMapper mapper,
            INetworkService networkService,
            ILogService logService,
            ILiveService liveService,
            IFileRecordService fileRecordService,
            IOptions<AppSettings> options
            )
        {
            _invoiceValidatorService = invoiceValidatorService;
            _mapper = mapper;
            _networkService = networkService;
            _logService = logService;
            _liveService = liveService;
            _fileRecordService = fileRecordService;
            _settings = options.Value;
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
                if (await _networkService.IsInternetAvailableAsync())
                {
                    var liveResponse = await _liveService.CreateInvoiceWithItemsAsync(dto);
                    if (liveResponse.StatusCode == ApiStatusCode.Success)
                    {
                        var record = await _fileRecordService.GetByInvoiceIdAsync(fiscalResponse.Data.InvoiceId);
                        if (record.StatusCode == ApiStatusCode.Success)
                        {
                            record.Data.IsSynced = (int)InvoiceStatus.Synced;
                            await _fileRecordService.UpdateFileRecordAsync(_mapper.Map<FileRecordDto>(record));
                        }
                    }
                }

                // ✅ 4. Return success if fiscal creation worked, but include validation info
                return new ApiResponse<InvoiceDto>(
                    isValid ? ApiStatusCode.Success : ApiStatusCode.Error,
                    isValid ? ResponseMessages.RecordSaved : "Invoice saved but failed validation",
                    null,
                    isValid ? string.Empty : string.Join(" | ", validation.ErrorMessages)
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(ResponseMessages.UnknownError, ex.InnerException?.Message ?? ex.Message);
            }

            // ----- Local helpers -----
            async Task LogError(string message) =>
                await _logService.LogAsync(
                    _logService.BuildLog(message, AlertType.Exception, "Invoice", nameof(CreateAsync)));

            ApiResponse<InvoiceDto> ErrorResponse(string msg, string err = "") =>
                new(ApiStatusCode.Error, msg, null, err);
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
                string invoiceNumber = GlobalMethods.InvoiceNumber(123111);
                //invoice.InvoiceNumber = invoiceNumber;

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
                await _logService.LogAsync(new Logs(errorMessage, AlertType.Exception, false));
                return new ApiResponse<(string, int)>(
                ApiStatusCode.Error,
                ResponseMessages.UnknownError,
                (string.Empty, 0),
                string.Empty);
            }
        }
    }
}