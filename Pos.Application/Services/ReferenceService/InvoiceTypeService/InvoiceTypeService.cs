using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Services.ReferenceService.LocalReferenceService;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ReferenceService.InvoiceTypeService
{
    public class InvoiceTypeService : IInvoiceTypeService
    {
        private readonly ILocalReferenceService<InvoiceType> _localReferenceService;

        public InvoiceTypeService(ILocalReferenceService<InvoiceType> localReferenceService)
        {
            _localReferenceService = localReferenceService;
        }

        public Task SyncInvoiceTypesAsync(string env, IEnumerable<ReferenceDto> serverInvoiceTypes)
            => _localReferenceService.SyncAsync(env, serverInvoiceTypes);

        public Task<ApiResponse<List<ReferenceDto>>> GetInvoiceTypesAsync()
            => _localReferenceService.GetAllAsync();

        public Task<ApiResponse<int>> GetInvoiceTypesCountAsync()
            => _localReferenceService.GetCountAsync();
    }
}
