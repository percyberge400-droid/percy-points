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
        [HttpPost("get-all-payment")]
        public async Task<IActionResult> GetAllPayment(string environment) =>
            Ok(await _referenceService.GetAllPaymentsAsync(environment));

        /// <summary>Get all invoice types for a given environment</summary>
        [HttpPost("get-all-invoice-type")]
        public async Task<IActionResult> GetAllInvoiceType(string environment) =>
            Ok(await _referenceService.GetAllInvoiceTypesAsync(environment));

        /// <summary>Get all services rendered for a given environment</summary>
        [HttpPost("get-all-service-rendered")]
        public async Task<IActionResult> GetServiceRendered(string environment) =>
            Ok(await _referenceService.GetAllServicesRenderedAsync(environment));
    }
}