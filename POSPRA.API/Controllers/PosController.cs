using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;

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
        public async Task<IActionResult> Status([FromBody] List<Logs> logs)
        {
            if (logs == null || !logs.Any())
                return BadRequest(new ApiResponse<string>("400", "No logs provided"));

            try
            {
                var result = await _posService.InsertPosStatusAsync(logs);

                if (result.Equals("Success", StringComparison.OrdinalIgnoreCase))
                    return Ok(new ApiResponse<string>("200", "Logs saved successfully", result));
                else
                    return StatusCode(500, new ApiResponse<string>("500", "Failed to save logs", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("500", ex.Message));
            }
        }


    }
}
