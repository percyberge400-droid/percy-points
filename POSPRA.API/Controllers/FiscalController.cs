using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.FiscalService;
using POSPRA.DTOs.InvoiceDTOs;

[Route("api/[controller]")]
[ApiController]
public class FiscalController : ControllerBase
{
    private readonly IFiscalService _fiscalService;

    public FiscalController(IFiscalService fiscalService)
    {
        _fiscalService = fiscalService;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _fiscalService.GetAllAsync());

    [HttpGet("GetAllUnsyncedAsync")]
    public async Task<IActionResult> GetUnSyncedAll() =>
      Ok(await _fiscalService.GetAllUnsyncedAsync());

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] InvoiceDto dto) =>
        Ok(await _fiscalService.CreateAsync(dto));
}
