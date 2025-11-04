using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;

namespace POSPRA.Application.Services.FileRecordService
{
    public interface IFileRecordService
    {
        Task<int> CreateAsync(long posId, string encryptedData, string invoiceNumber);
        Task<ApiResponse<List<FileRecordDto>>> GetAllAsync();
        Task<ApiResponse<List<FileRecordDto>>> GetAllUnsyncedAsync();
        Task<ApiResponse<FileRecordDto>> GetByInvoiceIdAsync(int invoiceId);
        Task<ApiResponse<FileRecordDto>> UpdateFileRecordAsync(FileRecordDto fileRecordDto);
        Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos);
    }
}