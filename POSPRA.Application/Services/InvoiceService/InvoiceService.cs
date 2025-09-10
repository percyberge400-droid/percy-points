using Newtonsoft.Json;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.Repositories.BaseRepository;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        private readonly InvoiceValidatorService _invoiceValidatorService;
        private readonly ILogService _logService;
        private readonly IRepository<FileRecord> _fileRecordRepository;
        public InvoiceService(InvoiceValidatorService invoiceValidatorService, ILogService logService, IRepository<FileRecord> fileRecordRepository)
        {
            _invoiceValidatorService = invoiceValidatorService;
            _logService = logService;
            _fileRecordRepository = fileRecordRepository;
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
                string invoiceNumber = GlobalMethods.InvoiceNumber(GlobalVariables.POS_ID);

                // Ensure invoice number is assigned
                //invoice.InvoiceNumber = invoiceNumber;

                string invoiceData = JsonConvert.SerializeObject(invoice);
                bool isValidCalculation = false;

                string payload = $"{invoiceData}|{isValidCalculation}|Latest";
                string signature = DataSigning.Sign(GlobalVariables.PRIVATE_KEY, payload);

                // Future: add encryption if required
                // byte[] eck = GlobalMethods.ToByteArray(GlobalVariables.ENCRYPTION_KEY);
                // string encryptedData = AESEncryption.Encrypt($"{payload}|{signature}", eck);

                int invoiceId = await InsertInvoiceAsync(invoice.BPOSID, payload, invoiceNumber);

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

        private async Task<int> InsertInvoiceAsync(int posId, string invoiceData, string invoiceNumber)
        {
            try
            {
                var model = new FileRecord
                {
                    POSID = posId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceData = invoiceData,
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
