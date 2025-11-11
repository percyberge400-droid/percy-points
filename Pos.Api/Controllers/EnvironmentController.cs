using Microsoft.AspNetCore.Mvc;
using Pos.Application.Interfaces;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentController : ControllerBase
    {
        private readonly IEnvironmentService _environmentService;

        public EnvironmentController(IEnvironmentService environmentService)
        {
            _environmentService = environmentService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var current = _environmentService.GetCurrentEnvironment();
            return Ok(new { environment = current.ToString() });
        }

        [HttpPost]
        public IActionResult Set([FromBody] string environment)
        {
            if (!Enum.TryParse<EnvironmentType>(environment, true, out var envType))
                return BadRequest("Invalid environment. Use 'Sandbox' or 'Production'.");

            _environmentService.SetCurrentEnvironment(envType);
            return Ok(new { message = $"Environment set to {envType}" });
        }
    }
}
