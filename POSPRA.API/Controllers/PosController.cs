using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.PosService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosController(IPosService posService) : ControllerBase
    {
        private readonly IPosService _posService = posService;

        [HttpPost("post")]
        public async Task<IActionResult> Post()
        {
            var response = await _posService.UpdateHeartBeatAsync();
            return Ok(response);
        }
    }
}
