using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.LiveService;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;

namespace POSPRA.API.Controllers
{
    /// <summary>
    /// Handles live operations such as saving decrypted invoice data
    /// and retrieving invoices in CSV format.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LiveController"/> class.
    /// </remarks>
    /// <param name="liveService">Service for live invoice operations.</param>
    [Route("api/[controller]")]
    [ApiController]
    public class LiveController(ILiveService liveService) : ControllerBase
    {
        private readonly ILiveService _liveService = liveService;

        /// <summary>
        /// Decrypts and saves a list of invoice records.
        /// </summary>
        /// <param name="dto">List of file records containing encrypted invoice data.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> with the service response after saving the invoices.
        /// </returns>
        [HttpPost("decrypt-save")]
        public async Task<IActionResult> Create([FromBody] List<FileRecordDto> dto) =>
            Ok(await _liveService.DecryptAndSaveInvoicesAsync(dto));

        /// <summary>
        /// Generates a CSV string of invoices based on the specified filter.
        /// </summary>
        /// <param name="dto">Filter criteria for retrieving invoices.</param>
        /// <returns>
        /// An <see cref="ApiResponse{String}"/> containing the CSV data of filtered invoices.
        /// </returns>
        [HttpPost("export-csv")]
        public async Task<ActionResult<ApiResponse<string>>> GetInvoicesCsv(InvoiceFilterDto dto)
        {
            var response = await _liveService.GetInvoicesCsvAsync(dto);
            return Ok(response);
        }
    }
}