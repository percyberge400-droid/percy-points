using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;

namespace Pos.Application.Services.ReferenceService.PaymentService
{
    public interface IPaymentService
    {
        Task SyncPaymentMethodsAsync(string env, IEnumerable<ReferenceDto> serverPayments);
        Task<ApiResponse<List<ReferenceDto>>> GetPaymentMethodsAsync();
        Task<ApiResponse<int>> GetPaymentMethodsCountAsync();
    }
}
