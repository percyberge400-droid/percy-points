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
    }
}