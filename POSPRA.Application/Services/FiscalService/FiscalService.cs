using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.UnitOfWork;
using InvoiceStatus = POSPRA.Application.Utility.GlobalEnums.InvoiceStatus;

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
        private readonly IRequestHeaderService _requestHeaderService;

        public FiscalService(InvoiceValidatorService invoiceValidatorService,
            ILogService logService,
            IFiscalRepository fileRecordRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IOptions<AppSettings> options,
            AutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRequestHeaderService requestHeaderService)
        {
            _invoiceValidatorService = invoiceValidatorService;
            _logService = logService;
            _fileRecordRepository = fileRecordRepository;
            _settings = options.Value;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _requestHeaderService = requestHeaderService;
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
                List<string> errors = new();
                if (dto == null)
                {
                    // Example inside a controller/service where you already have HttpContext
                    await _logService.LogAsync(
                      _logService.BuildLog(
                          Messages.INVALID_MODEL,
                          AlertType.Exception,
                          module: "Invoice",
                          action: nameof(CreateAsync)));


                    return new ApiResponse<InvoiceDto>(
                        statusCode: GlobalEnums.StatusCodes.Code_401.ToString(),
                        message: GlobalEnums.GetEnumDescription(GlobalEnums.StatusCodes.Code_401),
                        data: null, null
                        );
                }
                else
                {
                    // _mapper injected via constructor
                    var invoiceEntity = _mapper.Map<Invoice>(dto);
                    var validationResult = _invoiceValidatorService.ValidateInvoice(invoiceEntity);
                    string result = await CreateFiscalInvoiceAsync(invoiceEntity);
                    // send Invoice to live
                    //await _sendInvoiceToLiveService.CreateAsync(invoiceEntity);
                    if (validationResult.IsValid)
                    {
                        if (!String.IsNullOrEmpty(result))
                        {
                            return new ApiResponse<InvoiceDto>(
                                statusCode: ApiStatusCode.Success.ToString(),
                                message: ResponseMessages.RecordSaved,
                                data: null, null);
                        }
                        else
                        {
                            await _logService.LogAsync(
                             _logService.BuildLog(
                                 string.Format(Messages.INVOICE_NOT_AVAILABLE, " for " + dto.InvoiceType),
                                 AlertType.Exception,
                                 module: "Invoice",
                                 action: nameof(CreateAsync)));

                            return new ApiResponse<InvoiceDto>(
                                statusCode: ApiStatusCode.Error.ToString(),
                                message: ResponseMessages.UnknownError,
                                data: null, null);
                        }
                    }
                    else
                    {
                        await _logService.LogAsync(
                         _logService.BuildLog(
                             validationResult.ErrorMessages,
                             AlertType.Exception,
                             module: "Invoice",
                             action: nameof(CreateAsync)));

                    }
                }

                return new ApiResponse<InvoiceDto>(
                    statusCode: ApiStatusCode.Success.ToString(),
                    message: ResponseMessages.RecordSaved,
                    data: null
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<InvoiceDto>(
                    statusCode: ApiStatusCode.Error.ToString(),
                    message: ResponseMessages.UnknownError,
                    data: null,
                    errors: ex.InnerException?.Message ?? ex.Message
                );
            }
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
        public async Task<string> CreateFiscalInvoiceAsync(Invoice invoice)
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
                return invoiceId > 0 ? encryptedPackage : string.Empty;
            }
            catch (Exception ex)
            {
                string errorMessage = $"{GlobalVariables.DATE} CreateFiscalInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";
                await _logService.LogAsync(new Logs(errorMessage, AlertType.Exception, false));
                return string.Empty;
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

            return new ApiResponse<List<FileRecordDto>>(null, null, fileRecrodDTO, null);
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
                .ToList();

            // Map to DTOs
            var fileRecordDtos = _mapper.Map<List<FileRecordDto>>(unsynced);

            return new ApiResponse<List<FileRecordDto>>(null, null, fileRecordDtos, null);
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

        public async Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos)
        {
            if (fileRecordDtos == null || fileRecordDtos.Count == 0)
            {
                return new ApiResponse<List<FileRecordDto>>(
                    ApiStatusCode.Error.ToString(),
                    ResponseMessages.DataNotFound,
                    null,
                    null);
            }

            // Map DTOs to entities (assuming you have AutoMapper or manual mapping)
            var entities = _mapper.Map<List<FileRecord>>(fileRecordDtos);

            // Bulk update using the repository
            _fileRecordRepository.UpdateRange(entities);
            await _sqliteUnitOfWork.SaveChangesAsync();

            // Optionally map back to DTOs to return updated state
            var updatedDtos = _mapper.Map<List<FileRecordDto>>(entities);

            // ✅ Return in the same style you requested
            return new ApiResponse<List<FileRecordDto>>(
                ApiStatusCode.Success.ToString(),
                ResponseMessages.RecordSaved,
                updatedDtos,
                null);
        }
    }
}