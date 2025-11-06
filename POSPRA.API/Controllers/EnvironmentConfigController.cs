using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.EnvironmentConfigService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentConfigController(IEnvironmentConfigService environmentConfigService) : ControllerBase
    {
        private readonly IEnvironmentConfigService _environmentConfigService = environmentConfigService;

        [HttpPost("set-environment")]
        public IActionResult SetEnvironment([FromBody] bool flag)
        {
            _environmentConfigService.SetEnvironment(flag);
            return Ok(new { Message = $"Environment set to {(flag ? "Production" : "Sandbox")}" });
        }

    }
}
