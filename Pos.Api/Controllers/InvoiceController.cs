using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using Pos.Application.Services.InvoiceService;

namespace Pos.Local.API.Controllers
{
    [Route("api/IMSFiscal")]
    [ApiController]
    public class InvoiceController(IInvoiceService invoiceService, ISendInvoiceToCloudService sendInvoiceToCloudService) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;
        private readonly ISendInvoiceToCloudService _sendInvoiceToCloudService = sendInvoiceToCloudService;

        [HttpGet("GetInvoiceWithItems")]
        public async Task<IActionResult> GetInvoice(string invoiceNumber) =>
            Ok(await _invoiceService.GetInvoiceWithItems(invoiceNumber));

        [HttpPost("GetInvoiceNumberByModel")]
        public async Task<IActionResult> Create([FromBody] InvoiceDto dto) =>
            Ok(await _invoiceService.OldCreateAsync(dto, null));
    }
}