using Microsoft.AspNetCore.Mvc;
using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs.CommanDtos;

namespace Pos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [HttpPost("iscloud-syncenabled")]
        public async Task<IActionResult> IsCloudSyncEnabledAsync(GetByPosIdDto dto) =>
            Ok(await _configurationService.IsCloudSyncEnabledAsync(dto));

        [HttpGet("get-update-version")]
        public async Task<IActionResult> getUpdateVersion() =>
            Ok(await _configurationService.GetUpdateVersion());

        [HttpGet("get-updater-file")]
        public async Task<IActionResult> getUpdaterFile()
        {
            var result = await _configurationService.GetZipFileAsync();

            return Ok(new
            {
                fileName = result.fileName,
                base64 = result.base64
            });
        }
    }
}