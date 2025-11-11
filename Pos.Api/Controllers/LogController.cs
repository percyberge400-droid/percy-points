using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.LogService;
using Pos.Application.DTOs.LogDTOs;

namespace Pos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController(ILogService logService) : ControllerBase
    {

        private readonly ILogService _logService = logService;

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _logService.GetAllAsync());

        [HttpPost("create-logAsync")]
        public async Task<IActionResult> CreateLogAsync(CreateLogDto dto) =>
            Ok(await _logService.CreateLogAsync(dto));
    }
}