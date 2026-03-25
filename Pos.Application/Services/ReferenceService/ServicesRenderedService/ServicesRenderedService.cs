using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Services.ReferenceService.LocalReferenceService;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ReferenceService.ServicesRenderedService
{
    public class ServicesRenderedService : IServicesRenderedService
    {
        private readonly ILocalReferenceService<ServiceRendered> _localReferenceService;

        public ServicesRenderedService(ILocalReferenceService<ServiceRendered> localReferenceService)
        {
            _localReferenceService = localReferenceService;
        }

        public Task SyncServicesRenderedAsync(string env, IEnumerable<ReferenceDto> serverServices)
            => _localReferenceService.SyncAsync(env, serverServices);

        public Task<ApiResponse<List<ReferenceDto>>> GetServicesRenderedAsync()
            => _localReferenceService.GetAllAsync();

        public Task<ApiResponse<int>> GetServicesRenderedCountAsync()
            => _localReferenceService.GetCountAsync();
    }
}
