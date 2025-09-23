using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.Application.Services.LiveService
{
    public interface ILiveService
    {
        Task<ApiResponse<FileRecordDTO>> DecryptAndSaveInvoicesAsync(List<FileRecordDTO> dto);
    }
}
