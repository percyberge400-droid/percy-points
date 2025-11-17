using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.PageResponseDTOs;

namespace Pos.Application.Services.FileRecordService
{
    public interface IFileRecordService
    {
        Task<int> CreateAsync(long posId, string encryptedData, string invoiceNumber);
        Task<ApiResponse<List<FileRecordDto>>> GetAllUnsyncedAsync();
        Task<ApiResponse<PageResponseDto<FileRecordDto>>> GetAllAsync(GetAllFileRecordDto dto);
        Task<ApiResponse<FileRecordDto>> GetByInvoiceIdAsync(int invoiceId);
        Task<ApiResponse<FileRecordDto>> UpdateFileRecordAsync(FileRecordDto fileRecordDto);
        Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos);
    }
}