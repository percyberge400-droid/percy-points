using Microsoft.AspNetCore.Mvc;
using Pos.Application.Interfaces;
using Pos.Application.Services.ReferenceService;

namespace Pos.Cloud.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceController(IReferenceService referenceService) : ControllerBase
    {
        private readonly IReferenceService _referenceService = referenceService;

        /// <summary>Get all payments for a given environment</summary>
        [HttpPost("get-all-payment-methods")]
        public async Task<IActionResult> GetAllPayment([FromBody] string environment) =>
            Ok(await _referenceService.GetAllPaymentsAsync(environment));

        /// <summary>Get all invoice types for a given environment</summary>
        [HttpPost("get-all-invoice-types")]
        public async Task<IActionResult> GetAllInvoiceType([FromBody] string environment) =>
            Ok(await _referenceService.GetAllInvoiceTypesAsync(environment));

        /// <summary>Get all services rendered for a given environment</summary>
        [HttpPost("get-all-services-rendered")]
        public async Task<IActionResult> GetServiceRendered([FromBody] string environment) =>
            Ok(await _referenceService.GetAllServicesRenderedAsync(environment));
    }
}