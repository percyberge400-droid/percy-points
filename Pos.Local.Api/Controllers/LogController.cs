using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.LogService;
using Pos.DTOs.LogDTOs;

namespace Pos.Local.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController(ILogService logService) : ControllerBase
    {

        private readonly ILogService _logService = logService;

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll(GetAllLogsDto dto) =>
            Ok(await _logService.GetAllAsync(dto));

        [HttpPost("create-logAsync")]
        public async Task<IActionResult> CreateLogAsync(CreateLogDto dto) =>
            Ok(await _logService.CreateLogAsync(dto));
    }
}