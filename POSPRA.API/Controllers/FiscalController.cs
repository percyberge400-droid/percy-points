using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.FiscalService;

using POSPRA.Domain.Entities;
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
    /// Creates a new fiscal invoice.
    /// Accepts an <see cref="Invoice"/> object in the request body,
    /// processes it using <see cref="IFiscalService"/>, and returns the result.
    /// </summary>
    /// <param name="invoice">The invoice object to create.</param>
    /// <returns>An <see cref="IActionResult"/> containing the API response.</returns>
    [HttpPost("post")]
    public async Task<IActionResult> Post([FromBody] InvoiceDto dto)
    {
        var response = await _fiscalService.CreateAsync(dto);
        return Ok(response);
    }
}