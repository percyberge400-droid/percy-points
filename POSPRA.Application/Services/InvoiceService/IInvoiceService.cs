using POSPRA.Application.Utility;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.Application.Services.InvoiceService
{
    public interface IInvoiceService
    {
        Task<ApiResponse<List<POSVerificationDTO>>> POS_VerificationAsync(string bodyPosIds);
    }
}