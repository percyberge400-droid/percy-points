using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.Application.Services.InvoiceService
{
    public interface IInvoiceService
    {
        Task<ApiResponse<List<POSVerificationDTO>>> POS_VerificationAsync(string bodyPosIds);
    }
}