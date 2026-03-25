using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Services.ReferenceService.LocalReferenceService;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ReferenceService.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly ILocalReferenceService<Payment> _localReferenceService;

        public PaymentService(ILocalReferenceService<Payment> localReferenceService)
        {
            _localReferenceService = localReferenceService;
        }

        public Task SyncPaymentMethodsAsync(string env, IEnumerable<ReferenceDto> serverPayments)
            => _localReferenceService.SyncAsync(env, serverPayments);

        public Task<ApiResponse<List<ReferenceDto>>> GetPaymentMethodsAsync()
            => _localReferenceService.GetAllAsync();

        public Task<ApiResponse<int>> GetPaymentMethodsCountAsync()
            => _localReferenceService.GetCountAsync();
    }
}
