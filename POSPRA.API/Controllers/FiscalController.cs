using Microsoft.AspNetCore.Mvc;
using POSPRA.Domain.Entities;
using POSPRA.Repositories.FiscalRepository;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiscalController : ControllerBase
    {
        private readonly IFiscalRepository _fiscalRepository;
        public FiscalController(IFiscalRepository fiscalRepository)
        {
            _fiscalRepository = fiscalRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Logs model)
        {
            var response = "";//await _logService.CreateAsync(model);
            return Ok(response);
        }
    }
}
