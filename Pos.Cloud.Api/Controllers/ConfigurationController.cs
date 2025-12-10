using Microsoft.AspNetCore.Mvc;
using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs.CommanDtos;

namespace Pos.Cloud.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly IWebHostEnvironment _env;
        private readonly string _wwwrootPath;
        public ConfigurationController(IConfigurationService configurationService, IWebHostEnvironment env)
        {
            _configurationService = configurationService;
            _env = env;
            _wwwrootPath = Path.Combine(_env.WebRootPath, "Configurations");
        }

        [HttpPost("iscloud-syncenabled")]
        public async Task<IActionResult> IsCloudSyncEnabledAsync(GetByPosIdDto dto) =>
            Ok(await _configurationService.IsCloudSyncEnabledAsync(dto));


    }
}