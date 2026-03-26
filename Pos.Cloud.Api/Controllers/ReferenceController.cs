using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.ReferenceDtos.ReferenceRequest;
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
        public async Task<IActionResult> GetAllPayment([FromBody] ReferenceRequest environment) =>
            Ok(await _referenceService.GetAllPaymentsAsync(environment.Environment));

        /// <summary>Get all invoice types for a given environment</summary>
        [HttpPost("get-all-invoice-types")]
        public async Task<IActionResult> GetAllInvoiceType([FromBody] ReferenceRequest environment) =>
            Ok(await _referenceService.GetAllInvoiceTypesAsync(environment.Environment));

        /// <summary>Get all services rendered for a given environment</summary>
        [HttpPost("get-all-services-rendered")]
        public async Task<IActionResult> GetServiceRendered([FromBody] ReferenceRequest environment) =>
            Ok(await _referenceService.GetAllServicesRenderedAsync(environment.Environment));
    }
}