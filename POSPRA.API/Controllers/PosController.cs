using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.PosService;

namespace POSPRA.API.Controllers
{
    /// <summary>
    /// Controller to handle POS (Point of Sale) related endpoints.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PosController : ControllerBase
    {
        private readonly IPosService _posService;

        /// <summary>
        /// Initializes a new instance of <see cref="PosController"/>.
        /// </summary>
        /// <param name="posService">Service to handle POS operations.</param>
        public PosController(IPosService posService)
        {
            _posService = posService ?? throw new ArgumentNullException(nameof(posService));
        }

        /// <summary>
        /// Updates the POS heartbeat by calling the corresponding service method.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the API response.</returns>
        [HttpPost("HeartBeat")]
        public async Task<IActionResult> HeartBeat()
        {
            var response = await _posService.UpdateHeartBeatAsync();
            return Ok(response);
        }
    }
}
