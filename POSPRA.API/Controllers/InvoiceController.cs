using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;

        /// <summary>
        /// Creates a new fiscal invoice based on the provided invoice data.
        /// </summary>
        /// <param name="dto">
        /// The invoice information to be stored as a fiscal record.
        /// </param>
        /// <returns>
        /// The result of the create operation, including the saved invoice data.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] InvoiceDto dto) =>
            Ok(await _invoiceService.CreateAsync(dto));
    }
}
