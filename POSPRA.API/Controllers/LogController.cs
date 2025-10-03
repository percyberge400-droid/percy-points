using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.LogService;

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

        [HttpGet("GetAllCloudAsync")]
        public async Task<IActionResult> GetAllCloudAsync() =>
            Ok(await _logService.GetAllCloudAsync());
    }
}
