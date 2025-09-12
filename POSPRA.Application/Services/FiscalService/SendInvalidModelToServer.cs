using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.FiscalService
{
    /// <summary>
    /// Sends invoice-related models to the server, e.g., invalid invoices.
    /// </summary>
    public class SendModelToServer
    {
        private readonly IHttpService _httpService;
        private readonly AppSettings _settings;

        /// <summary>
        /// Initializes a new instance of <see cref="SendModelToServer"/>.
        /// </summary>
        /// <param name="httpService">HTTP service for sending requests.</param>
        /// <param name="options">Application settings injected via IOptions.</param>
        public SendModelToServer(IHttpService httpService, IOptions<AppSettings> options)
        {
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Sends an invalid invoice model to the server for logging or reporting.
        /// </summary>
        /// <param name="invoice">The invoice to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Throws any exception encountered during HTTP call.</exception>
        public async Task SendInvalidModelToServer(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            try
            {
                // Convert invoice to Logs object
                var logs = new List<Logs>
                {
                    new(JsonConvert.SerializeObject(invoice), (int)AlertType.InvalidInvoiceModel, false)
                };

                // Prepare custom headers
                var customHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {_settings.Token}" },
                    { "POSID", _settings.POS.ToString() },
                    { "MACADDRESS", _settings.LICENSEKEY?.ToString() ?? string.Empty }
                };

                // Send the serialized invoice logs to the endpoint
                StatusDTO res = await _httpService.PostAsync<StatusDTO>(Endpoints.POSStatus, logs, customHeaders);
            }
            catch (Exception ex)
            {
                // Optional: you can log ex here before rethrowing
                throw;
            }
        }
    }
}
