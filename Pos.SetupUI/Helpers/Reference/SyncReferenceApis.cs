using Newtonsoft.Json;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Services.ReferenceService.InvoiceTypeService;
using Pos.Application.Services.ReferenceService.PaymentService;
using Pos.Application.Services.ReferenceService.ServicesRenderedService;
using Pos.Application.Utility;
using Pos.Domain.Entities;
using System.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace Pos.SetupUI.Helpers.Reference
{
    public class SyncReferenceApis
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceTypeService _invoiceTypeService;
        private readonly IServicesRenderedService _servicesRenderedService;
        private readonly bool _isServiceAvailable;
        private readonly Action<string, bool, bool> _showMessage;

        public SyncReferenceApis(
            IPaymentService paymentService,
            IInvoiceTypeService invoiceTypeService,
            IServicesRenderedService servicesRenderedService,
            bool isServiceAvailable,
            Action<string, bool, bool> showMessage)
        {
            _paymentService = paymentService;
            _invoiceTypeService = invoiceTypeService;
            _servicesRenderedService = servicesRenderedService;
            _isServiceAvailable = isServiceAvailable;
            _showMessage = showMessage;
        }

        public async Task SyncAllAfterDbSetupAsync(string selectedEnvironment, string password)
        {
            //if (!_isServiceAvailable)
            //    return;

            await SyncPaymentsAfterDbSetupAsync(selectedEnvironment, password);
            await SyncInvoiceTypesAfterDbSetupAsync(selectedEnvironment, password);
            await SyncServicesRenderedAfterDbSetupAsync(selectedEnvironment, password);
        }

        private async Task SyncPaymentsAfterDbSetupAsync(string selectedEnvironment, string password)
        {
            try
            {
                _showMessage("Retrieving payment methods from server...", true, false);

                var json = await PostAsync(Endpoints.GetAllPaymentMethods, selectedEnvironment, password);
                if (json == null) return;

                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<ReferenceDto>>>(json);
                var entities = wrapper?.Data;

                if (entities == null || !entities.Any())
                {
                    _showMessage("No payment methods returned from server.", true, false);
                    return;
                }

                await _paymentService.SyncPaymentMethodsAsync(selectedEnvironment, entities);

                _showMessage("Payment methods synchronized successfully.", true, false);
            }
            catch (Exception ex)
            {
                _showMessage($"Error syncing payment methods: {ex.Message}", false, false);
            }
        }

        private async Task SyncInvoiceTypesAfterDbSetupAsync(string selectedEnvironment, string password)
        {
            try
            {
                _showMessage("Retrieving invoice types from server...", true, false);

                var json = await PostAsync(Endpoints.GetAllInvoiceTypes, selectedEnvironment, password);
                if (json == null) return;

                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<ReferenceDto>>>(json);
                var entities = wrapper?.Data;

                if (entities == null || !entities.Any())
                {
                    _showMessage("No invoice types returned from server.", true, false);
                    return;
                }

                await _invoiceTypeService.SyncInvoiceTypesAsync(selectedEnvironment, entities);

                _showMessage("Invoice types synchronized successfully.", true, false);
            }
            catch (Exception ex)
            {
                _showMessage($"Error syncing invoice types: {ex.Message}", false, false);
            }
        }

        private async Task SyncServicesRenderedAfterDbSetupAsync(string selectedEnvironment, string password)
        {
            try
            {
                _showMessage("Retrieving services rendered from server...", true, false);

                var json = await PostAsync(Endpoints.GetAllServicesRendered, selectedEnvironment, password);
                if (json == null) return;

                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<ReferenceDto>>>(json);
                var entities = wrapper?.Data;

                if (entities == null || !entities.Any())
                {
                    _showMessage("No services rendered returned from server.", true, false);
                    return;
                }

                await _servicesRenderedService.SyncServicesRenderedAsync(selectedEnvironment, entities);

                _showMessage("Services rendered synchronized successfully.", true, false);
            }
            catch (Exception ex)
            {
                _showMessage($"Error syncing services rendered: {ex.Message}", false, false);
            }
        }

        // Shared HTTP helper to avoid duplication
        private async Task<string?> PostAsync(string endpoint, string selectedEnvironment, string password)
        {
            //var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "";
            var baseUrl = "https://localhost:7020/";
            var apiUrl = $"{baseUrl}{endpoint}";

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                _showMessage("API URL is missing in configuration.", false, false);
                return null;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(selectedEnvironment),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", password);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}