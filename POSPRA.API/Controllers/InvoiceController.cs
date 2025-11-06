using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.API.Controllers
{
    [Route("api/IMSFiscal")]
    [ApiController]
    public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;

        [HttpGet("GetInvoiceWithItems")]
        public async Task<IActionResult> GetInvoice(string invoiceNumber) =>
            Ok(await _invoiceService.GetInvoiceWithItems(invoiceNumber));

        [HttpPost("GetInvoiceNumberByModel")]
        public async Task<IActionResult> Create([FromBody] InvoiceDto dto) =>
            Ok(await _invoiceService.CreateAsync(dto));
    }
}
