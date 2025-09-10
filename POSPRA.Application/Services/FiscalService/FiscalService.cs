using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.Repositories.BaseRepository;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.FiscalService
{
    public class FiscalService : IFiscalService
    {
        private readonly InvoiceValidatorService _invoiceValidatorService;
        private readonly ILogService _logService;
        private readonly IRepository<FileRecord> _fileRecordRepository;
        private readonly AppSettings _settings;

        public FiscalService(InvoiceValidatorService invoiceValidatorService,
            ILogService logService,
            IRepository<FileRecord> fileRecordRepository,
            IOptions<AppSettings> options)
        {
            _invoiceValidatorService = invoiceValidatorService;
            _logService = logService;
            _fileRecordRepository = fileRecordRepository;
            _settings = options.Value;
        }

        public async Task<ApiResponse<Invoice>> CreateAsync(Invoice invoice)
        {
            try
            {
                List<string> errors = new();
                if (invoice == null)
                {
                    await _logService.LogAsync(new Logs(GlobalVariables.DATE + Messages.INVALID_MODEL, (int)AlertType.Exception, false), 2);

                    return new ApiResponse<Invoice>(
                        statusCode: GlobalEnums.StatusCodes.Code_401.ToString(),
                        message: GlobalEnums.GetEnumDescription(GlobalEnums.StatusCodes.Code_401),
                        data: null, null
                        );
                }

                var isValid = _invoiceValidatorService.InvoiceValidator(invoice, errors);
                if (isValid)
                {
                    string result = await CreateFiscalInvoiceAsync(invoice);
                    if (!String.IsNullOrEmpty(result))
                    {
                        return new ApiResponse<Invoice>(
                            statusCode: GlobalEnums.StatusCodes.Code_100.ToString(),
                            message: GlobalEnums.GetEnumDescription(GlobalEnums.StatusCodes.Code_100),
                            data: null, null);
                    }
                    else
                    {
                        await _logService.LogAsync(
                            new Logs(GlobalVariables.DATE + string.Format(Messages.INVOICE_NOT_AVAILABLE, " for " + invoice.InvoiceType),
                            (int)AlertType.Exception, false),
                            2);

                        return new ApiResponse<Invoice>(
                            statusCode: GlobalEnums.StatusCodes.Code_101.ToString(),
                            message: GlobalEnums.GetEnumDescription(GlobalEnums.StatusCodes.Code_101),
                            data: null, null);
                    }
                }

                return new ApiResponse<Invoice>(
                    statusCode: "200",
                    message: "Created successfully",
                    data: null
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<Invoice>(
                    statusCode: "500",
                    message: "An error occurred while creating entity",
                    data: null,
                    errors: ex.InnerException?.Message ?? ex.Message
                );
            }
        }

        public async Task<string> CreateFiscalInvoiceAsync(Invoice invoice)
        {
            try
            {
                string invoiceNumber = GlobalMethods.InvoiceNumber(_settings.POS);

                // Ensure invoice number is assigned
                //invoice.InvoiceNumber = invoiceNumber;

                string invoiceData = JsonConvert.SerializeObject(invoice);
                bool isValidCalculation = false;

                string payload = $"{invoiceData}|{isValidCalculation}|Latest";
                string signature = DataSigning.Sign(_settings.PV, payload);

                // Generate 256-bit key once & reuse (store securely!)
                byte[] key = Encoding.UTF8.GetBytes(_settings.EC.PadRight(32).Substring(0, 32));

                // Encrypt with AES-GCM
                var encryptedData = ModernAESEncryption.Encrypt($"{payload}|{signature}", key);
                string encryptedPackage = $"{encryptedData.cipherText}:{encryptedData.nonce}:{encryptedData.tag}";

                int invoiceId = await InsertInvoiceAsync(invoice.BPOSID, encryptedPackage, invoiceNumber);

                return invoiceId > 0 ? invoiceNumber : string.Empty;
            }
            catch (Exception ex)
            {
                string errorMessage =
                    $"{GlobalVariables.DATE} CreateFiscalInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.LogAsync(new Logs(errorMessage, (int)AlertType.Exception, false));
                return string.Empty;
            }
        }

        private async Task<int> InsertInvoiceAsync(int posId, string encryptedData, string invoiceNumber)
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
                return model.ID;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} InsertInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.LogAsync(new Logs(errorMessage, (int)AlertType.Exception, false));
                return 0;
            }
        }
    }
}
