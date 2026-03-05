using Microsoft.AspNetCore.Mvc;
using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.DTOs.ConfigurationsDtos;

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

        [HttpPost("get-update-version")]
        public async Task<IActionResult> getUpdateVersion(GetByModuleDto dto)
        {
            return Ok(await _configurationService.GetUpdateVersion(_wwwrootPath, dto));
        }

        [HttpPost("get-updater-file")]
        public async Task<IActionResult> getUpdaterFile(GetByModuleDto dto)
        {
            return Ok(await _configurationService.GetZipFileAsync(_wwwrootPath, dto));
        }
    }
}