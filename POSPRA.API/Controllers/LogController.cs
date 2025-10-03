using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.LogService;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController(ILogService logService) : ControllerBase
    {

        private readonly ILogService _logService = logService;

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _logService.GetAllAsync());

        [HttpGet("cloud-logs")]
        public async Task<IActionResult> GetCloudLogs() =>
            Ok(await _logService.GetAllCloudAsync());

        [HttpPost("create-cloud-logs")]
        public async Task<IActionResult> CreateCloudLog([FromBody] List<LogDto> dto) =>
            Ok(await _logService.CreateCloudLog(dto));
    }
}
