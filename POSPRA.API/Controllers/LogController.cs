using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.LogService;
using POSPRA.DTOs.LogDTOs;

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

        [HttpGet("create-logAsync")]
        public async Task<IActionResult> CreateLogAsync(CreateLogDto dto) =>
            Ok(await _logService.CreateLogAsync(dto));
    }
}