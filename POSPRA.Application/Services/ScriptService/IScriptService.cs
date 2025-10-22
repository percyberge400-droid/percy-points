using POSPRA.DTOs;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.Services.ScriptService
{
    public interface IScriptService
    {
        Task<ApiResponse<ScriptDTO>> CreateScript(ScriptDTO dto);
    }
}
