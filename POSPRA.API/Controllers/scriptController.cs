using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.ScriptService;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class scriptController(IScriptService scriptService) : ControllerBase
    {
        private readonly IScriptService _scriptService = scriptService;

        [HttpPost("createScript")]
        public async Task<IActionResult> Create([FromBody] ScriptDTO dto) =>
            Ok(await _scriptService.CreateScript(dto));
    }
}
