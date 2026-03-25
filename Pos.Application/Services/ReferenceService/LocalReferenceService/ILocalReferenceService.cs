using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using System.Collections.Generic;

namespace Pos.Application.Services.ReferenceService.LocalReferenceService
{
    public interface ILocalReferenceService<TEntity>
        where TEntity : class
    {
        Task SyncAsync(string env, IEnumerable<ReferenceDto> serverDtos);
        Task<ApiResponse<List<ReferenceDto>>> GetAllAsync();
        Task<ApiResponse<int>> GetCountAsync();
    }
}
