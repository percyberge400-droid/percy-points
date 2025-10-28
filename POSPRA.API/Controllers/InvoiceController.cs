using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController(IInvoiceService invoiceService, ISendInvoiceToCloudService sendInvoiceToCloudService) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;
        private readonly ISendInvoiceToCloudService _sendInvoiceToCloudService = sendInvoiceToCloudService;

        [HttpGet("getInvoiceWithItems")]
        public async Task<IActionResult> GetInvoice(string invoiceNumber) =>
            Ok(await _invoiceService.GetInvoiceWithItems(invoiceNumber));

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] InvoiceDto dto) =>
            Ok(await _invoiceService.CreateAsync(dto));

        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] CreateLogDto dto) =>
        //  Ok(await _sendInvoiceToCloudService.SyncInvoicesAsync(dto));
    }
}
