using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDTOs;

namespace Pos.Application.Services.ScriptService
{
    public interface IScriptService
    {
        Task<ApiResponse<ScriptDTO>> CreateScript(ScriptDTO dto);
    }
}
