using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.Services.HelperService;
using Pos.Application.Services.PosService;

namespace Pos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosController(IPosService posService, IRequestHeaderService requestHeaderService) : ControllerBase
    {
        private readonly IPosService _posService = posService;
        private readonly IRequestHeaderService _requestHeaderService = requestHeaderService;
        /// <summary>
        ///Get the heartbeat of POS Systems
        /// </summary>
        /// <returns>
        /// Heartbeat Status
        /// </returns>
        [HttpPost("HeartBeat")]
        public async Task<IActionResult> HeartBeat(GetByPosIdDto dto)
        {
            var response = await _posService.UpdateHeartBeatAsync(dto.PosId, dto.Environment);
            return Ok(response);
        }

        /// <summary>
        ///Get the lastest configuration from Server
        /// </summary>
        /// <returns>
        /// List of configuration values
        /// </returns>
        [HttpPost("Configuration")]
        public async Task<IActionResult> Configuration(string env)
        {
            var response = await _posService.GetConfigurationsAsync(env);
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
