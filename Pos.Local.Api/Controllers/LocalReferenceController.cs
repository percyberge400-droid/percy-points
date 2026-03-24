using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.ReferenceService.InvoiceTypeService;
using Pos.Application.Services.ReferenceService.PaymentService;
using Pos.Application.Services.ReferenceService.ServicesRenderedService;

namespace Pos.Local.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalReferenceController(
        IPaymentService paymentService,
        IInvoiceTypeService invoiceTypeService,
        IServicesRenderedService servicesRenderedService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IInvoiceTypeService _invoiceTypeService = invoiceTypeService;
        private readonly IServicesRenderedService _servicesRenderedService = servicesRenderedService;

        [HttpGet("getallpaymentmethods")]
        public async Task<IActionResult> GetAllPaymentMethodsAsync() =>
            Ok(await _paymentService.GetPaymentMethodsAsync());

        [HttpGet("getallinvoicetypes")]
        public async Task<IActionResult> GetAllInvoiceTypesAsync() =>
            Ok(await _invoiceTypeService.GetInvoiceTypesAsync());

        [HttpGet("getallservicesrendered")]
        public async Task<IActionResult> GetAllServicesRenderedAsync() =>
            Ok(await _servicesRenderedService.GetServicesRenderedAsync());
    }
}