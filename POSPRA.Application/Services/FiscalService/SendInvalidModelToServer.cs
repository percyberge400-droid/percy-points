using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs.InvoiceDTOs;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.FiscalService
{
    public class SendModelToServer
    {
        private readonly IHttpService _httpService;
        private readonly AppSettings _settings;
        public SendModelToServer(IHttpService httpService, IOptions<AppSettings> options)
        {
            _httpService = httpService;
            _settings = options.Value;
        }
        public async Task SendInvalidModelToServer(Invoice invoice)
        {
            try
            {
                // StatusModel => StatusDTO
                var result = new List<Logs>
                {
                    new Logs(JsonConvert.SerializeObject(invoice), (int)AlertType.InvalidInvoiceModel, false)
                };

                var customHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {_settings.Token}" },
                    { "POSID", GlobalVariables.POS_ID.ToString() },
                    { "MACADDRESS", GlobalVariables.LICENSE_KEY.ToString() }
                };

                StatusDTO res = await _httpService.PostAsync<StatusDTO>(Endpoints.POSStatus, result, customHeaders);

            }
            catch { }
        }
    }
}
