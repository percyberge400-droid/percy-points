using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.InvoiceService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;

        [HttpGet("getInvoiceWithItems")]
        public async Task<IActionResult> GetInvoice(string invoiceNumber) =>
            Ok(await _invoiceService.GetInvoiceWithItems(invoiceNumber));
    }
}
