using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.PosService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosController(IPosService posService) : ControllerBase
    {
        private readonly IPosService _posService = posService;

        [HttpPost("HeartBeat")]
        public async Task<IActionResult> HeartBeat()
        {
            var response = await _posService.UpdateHeartBeatAsync();
            return Ok(response);
        }
    }
}
