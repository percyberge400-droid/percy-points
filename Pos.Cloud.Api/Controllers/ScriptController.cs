using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.ScriptService;

namespace Pos.Cloud.Api.Controllers
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