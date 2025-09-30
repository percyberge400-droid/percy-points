using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Service responsible for handling fiscal invoice operations.
    /// It validates invoices, generates invoice numbers, signs and encrypts invoice data,
    /// logs errors or important events, and persists invoice records to the database.
    /// </summary>
    public class FiscalService : IFiscalService
    {
        private readonly InvoiceValidatorService _invoiceValidatorService;
        private readonly ILogService _logService;
        private readonly IFiscalRepository _fileRecordRepository;
        private readonly AppSettings _settings;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ILiveService _liveService;
        private readonly INetworkService _networkService;

        public FiscalService(InvoiceValidatorService invoiceValidatorService,
            ILogService logService,
            IFiscalRepository fileRecordRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IOptions<AppSettings> options,
            AutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor
,
            ILiveService liveService,
            INetworkService networkService)
        {
            _invoiceValidatorService = invoiceValidatorService;
            _logService = logService;
            _fileRecordRepository = fileRecordRepository;
            _settings = options.Value;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _liveService = liveService;
            _networkService = networkService;
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
                var fiscalResponse = await CreateFiscalInvoiceAsync(invoiceEntity);
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
                        var record = await _fileRecordRepository.GetByIdAsync(fiscalResponse.Data.InvoiceId);
                        if (record != null)
                        {
                            record.IsSynced = (int)InvoiceStatus.Synced;
                            await UpdateFileRecordAsync(_mapper.Map<FileRecordDto>(record));
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
        public async Task<ApiResponse<(string EncryptedPackage, int InvoiceId)>> CreateFiscalInvoiceAsync(Invoice invoice)
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
                int invoiceId = await InsertInvoiceAsync(invoice.POSID, encryptedPackage, invoiceNumber);

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

        /// <summary>
        /// Retrieves **all** file records from the repository.
        /// </summary>
        /// <remarks>
        /// This method fetches every record regardless of sync status,
        /// maps them to <see cref="FileRecordDTO"/>, and returns the list
        /// wrapped in an <see cref="ApiResponse{T}"/>.
        /// </remarks>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of all
        /// <see cref="FileRecordDTO"/> objects.
        /// </returns>
        public async Task<ApiResponse<List<FileRecordDto>>> GetAllAsync()
        {
            var output = await _fileRecordRepository.GetAllAsync();
            var fileRecrodDTO = _mapper.Map<List<FileRecordDto>>(output);

            return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, fileRecrodDTO, string.Empty);
        }

        /// <summary>
        /// Retrieves only the file records that are **not yet synced**.
        /// </summary>
        /// <remarks>
        /// The method first obtains all records from the repository, filters them
        /// in memory to include only those whose <c>IsSynced</c> property equals
        /// <see cref="InvoiceStatus.NotSynced"/>, then maps the result to
        /// <see cref="FileRecordDTO"/> objects.
        /// </remarks>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of unsynced
        /// <see cref="FileRecordDTO"/> objects.
        /// </returns>
        public async Task<ApiResponse<List<FileRecordDto>>> GetAllUnsyncedAsync()
        {
            // Await the repository call directly (don't use .Result)
            var allRecords = await _fileRecordRepository.GetAllAsync();

            // Filter in memory for unsynced records
            var unsynced = allRecords
                .Where(x => x.IsSynced == (int)InvoiceStatus.NotSynced)
                .Take(1000)
                .ToList();

            // Map to DTOs
            var fileRecordDtos = _mapper.Map<List<FileRecordDto>>(unsynced);
            if (fileRecordDtos.Any())
                return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, fileRecordDtos, string.Empty);

            return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }

        /// <summary>
        /// Inserts the encrypted invoice data into the database as a FileRecord.
        /// </summary>
        /// <param name="posId">The POS identifier for the invoice.</param>
        /// <param name="encryptedData">The encrypted invoice data.</param>
        /// <param name="invoiceNumber">The generated invoice number.</param>
        /// <returns>
        /// The ID of the newly created FileRecord if successful; otherwise, 0.
        /// </returns>
        private async Task<int> InsertInvoiceAsync(long posId, string encryptedData, string invoiceNumber)
        {
            try
            {
                var model = new FileRecord
                {
                    POSID = posId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceData = encryptedData,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    IsSynced = (int)InvoiceStatus.NotSynced,
                    AttemptCount = 0
                };

                await _fileRecordRepository.AddAsync(model);
                await _sqliteUnitOfWork.SaveChangesAsync();
                return model.ID;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} InsertInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.LogAsync(new Logs(errorMessage, AlertType.Exception, false));
                return 0;
            }
        }

        /// <summary>
        /// Updates multiple <see cref="FileRecordDto"/> objects in the database.
        /// </summary>
        /// <param name="fileRecordDtos">
        /// A list of <see cref="FileRecordDto"/> items to update.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of updated <see cref="FileRecordDto"/> 
        /// objects when successful, or an error response if validation fails or no records exist.
        /// </returns>
        public async Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos, bool isSingle)
        {
            if (fileRecordDtos == null || fileRecordDtos.Count == 0)
            {
                return new ApiResponse<List<FileRecordDto>>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null,
                    string.Empty);
            }

            var entities = _mapper.Map<List<FileRecord>>(fileRecordDtos);
            if (!isSingle)
                _fileRecordRepository.UpdateRange(entities);
            await _sqliteUnitOfWork.SaveChangesAsync();

            var updatedDtos = _mapper.Map<List<FileRecordDto>>(entities);

            return new ApiResponse<List<FileRecordDto>>(
                ApiStatusCode.Success,
                ResponseMessages.RecordSaved,
                updatedDtos,
                string.Empty);
        }

        /// <summary>
        /// Updates a single <see cref="FileRecordDto"/> in the database.
        /// </summary>
        /// <param name="fileRecordDto">
        /// The <see cref="FileRecordDto"/> object to update.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the updated <see cref="FileRecordDto"/> 
        /// when successful, or an error response if validation fails or the update does not succeed.
        /// </returns>
        public async Task<ApiResponse<FileRecordDto>> UpdateFileRecordAsync(FileRecordDto fileRecordDto)
        {
            if (fileRecordDto == null)
            {
                return new ApiResponse<FileRecordDto>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null,
                    string.Empty);
            }

            var result = await UpdateFileRecordsAsync(new List<FileRecordDto> { fileRecordDto }, true);

            // Return a single item if update succeeded, otherwise an error response
            return result.StatusCode == ApiStatusCode.Success && result.Data?.Count > 0
                ? new ApiResponse<FileRecordDto>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordSaved,
                    result.Data[0],
                    string.Empty)
                : new ApiResponse<FileRecordDto>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null,
                    string.Empty);
        }
    }
}