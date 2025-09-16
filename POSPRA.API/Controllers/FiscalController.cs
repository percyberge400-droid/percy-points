using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.FiscalService;
using POSPRA.DTOs.InvoiceDTOs;

/// <summary>
/// Controller responsible for handling fiscal invoice-related API endpoints.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FiscalController"/> class.
/// </remarks>
/// <param name="fiscalService">The fiscal service used to process invoices.</param>
[Route("api/[controller]")]
[ApiController]
public class FiscalController(IFiscalService fiscalService) : ControllerBase
{
    private readonly IFiscalService _fiscalService = fiscalService;

    /// <summary>
    /// Retrieves all fiscal invoices.
    /// </summary>
    /// <remarks>
    /// Returns a list of all fiscal invoices stored in the system.
    /// </remarks>
    /// <response code="200">A collection of fiscal invoices.</response>
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _fiscalService.GetAllAsync());

    /// <summary>
    /// Creates a new fiscal invoice.
    /// </summary>
    /// <param name="dto">
    /// The <see cref="InvoiceDto"/> containing the invoice details to be created.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with the result of the create operation.
    /// </returns>
    /// <response code="200">The newly created invoice or a success message.</response>
    /// <response code="400">If the input data is invalid.</response>
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] InvoiceDto dto) =>
        Ok(await _fiscalService.CreateAsync(dto));

}