using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;

namespace Pos.Application.Services.ReferenceService.ServicesRenderedService
{
    public interface IServicesRenderedService
    {
        Task SyncServicesRenderedAsync(string env, IEnumerable<ReferenceDto> serverServices);
        Task<ApiResponse<List<ReferenceDto>>> GetServicesRenderedAsync();
        Task<ApiResponse<int>> GetServicesRenderedCountAsync();
    }
}