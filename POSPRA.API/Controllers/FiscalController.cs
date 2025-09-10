using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Domain.Entities;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiscalController : ControllerBase
    {
        private readonly IFiscalService _fiscalService;
        public FiscalController(IFiscalService fiscalService)
        {
            _fiscalService = fiscalService;
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] Invoice invoice)
        {
            var response = await _fiscalService.CreateAsync(invoice);

            return Ok(response);
        }
    }
}
