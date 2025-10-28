using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.ScriptService;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScriptController(IScriptService scriptService) : ControllerBase
    {
        private readonly IScriptService _scriptService = scriptService;

        [HttpPost("create-script")]
        public async Task<IActionResult> CreateScript(ScriptDTO dto) =>
            Ok(await _scriptService.CreateScript(dto));
    }
}
