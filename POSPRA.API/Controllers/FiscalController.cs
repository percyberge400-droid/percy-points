using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.FiscalService;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.ProductCatalogDtos;

[Route("api/[controller]")]
[ApiController]
public class FiscalController(IFiscalService fiscalService) : ControllerBase
{
    private readonly IFiscalService _fiscalService = fiscalService;

    /// <summary>
    /// Retrieves all fiscal invoices from the database.
    /// </summary>
    /// <returns>
    /// A list of all invoices with their fiscal details.
    /// </returns>
    [HttpGet("getalls")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _fiscalService.GetAllAsync());

    /// <summary>
    /// Retrieves all fiscal invoices that have not yet been synced.
    /// </summary>
    /// <returns>
    /// A list of unsynced invoices with their fiscal details.
    /// </returns>
    [HttpGet("getallunsynced")]
    public async Task<IActionResult> GetUnSyncedAll() =>
        Ok(await _fiscalService.GetAllUnsyncedAsync());

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
        Ok(await _fiscalService.CreateAsync(dto));

    /// <summary>
    /// This method is used to create product catalogue in SQLite.
    /// </summary>
    /// <param name="productCatalogueDto"></param>
    /// <returns></returns>
    [HttpPost("postProductCatalogue")]
    public async Task<IActionResult> Create([FromBody] ProductCatalogueDto productCatalogueDto) =>
        Ok(await _fiscalService.PostProductCatalog(productCatalogueDto));

    /// <summary>
    /// This API is used to get product catalog.
    /// </summary>
    /// <param name="productCatalogueDto"></param>
    /// <returns></returns>
    [HttpGet("GetProductCatalogue")]
    public async Task<IActionResult> GetProductCatalogue() =>
        Ok(await _fiscalService.GetProductCatalogue());
}
