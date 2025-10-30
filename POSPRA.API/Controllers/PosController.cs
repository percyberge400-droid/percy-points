using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosController(IPosService posService, IRequestHeaderService requestHeaderService) : ControllerBase
    {
        private readonly IPosService _posService = posService;
        private readonly IRequestHeaderService _requestHeaderService = requestHeaderService;
        /// <summary>
        ///Get the heartbeat of POS System
        /// </summary>
        /// <returns>
        /// Heartbeat Status
        /// </returns>
        [HttpPost("HeartBeat")]
        public async Task<IActionResult> HeartBeat()
        {
            //var posId = _requestHeaderService.GetPosId();
            var response = await _posService.UpdateHeartBeatAsync(133013);
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
