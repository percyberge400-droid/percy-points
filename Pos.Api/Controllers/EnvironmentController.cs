using Microsoft.AspNetCore.Mvc;
using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;

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

        // GET api/environment
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var current = await _environmentService.GetCurrentEnvironmentAsync();
            return Ok(new { environment = current.ToString() });
        }

        // POST api/environment
        [HttpPost]
        public IActionResult Set([FromBody] string environment)
        {
            if (!Enum.TryParse<EnvironmentType>(environment, true, out var envType))
                return BadRequest("Invalid environment. Use 'Sandbox' or 'Production'.");

            _environmentService.SetCurrentEnvironment(envType);

            return Ok(new { message = $"Environment set to {envType} for this process" });
        }
    }
}
