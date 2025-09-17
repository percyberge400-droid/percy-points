using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.PosService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosController(IPosService posService) : ControllerBase
    {
        private readonly IPosService _posService = posService;

        /// <summary>
        ///Get the heartbeat of POS System
        /// </summary>
        /// <returns>
        /// Heartbeat Status
        /// </returns>
        [HttpPost("HeartBeat")]
        public async Task<IActionResult> HeartBeat()
        {
            var response = await _posService.UpdateHeartBeatAsync();
            return Ok(response);
        }

        /// <summary>
        ///Get the lastest configuration from Server
        /// </summary>
        /// <returns>
        /// List of configuration values
        /// </returns>
        [HttpPost("Configuration")]
        public async Task<IActionResult> Configuration()
        {
            var response = await _posService.GetConfigurationsAsync();
            return Ok(response);
        }

        /// <summary>
        /// Post IMS Component Log
        /// </summary>
        /// <returns>
        /// Response from Server
        /// </returns>
        [HttpPost("Status")]
        public async Task<IActionResult> Status()
        {
            var response = await _posService.InsertPosStatusAsync();
            return Ok(response);
        }
    }
}
