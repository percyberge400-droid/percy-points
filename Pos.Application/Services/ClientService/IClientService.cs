using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ClientService
{
    public interface IClientService
    {
        Task<ApiResponse<PosClients>> GetByMacAsync(ClientValidationDto dto);
        Task<string> UpdateConfigurationFlag(bool isConfiguration, long? posId, string environment);
        Task<bool> IsServiceEnabled(long posId, string env);
        Task<LogResponseDto> IsLogEnabled(long posId, string env);
        Task<bool> DisablePosCLientLogBit(string env, long posId);
        Task<IEnumerable<InvoiceType>> GetInvoiceType(string env);
    }
}
