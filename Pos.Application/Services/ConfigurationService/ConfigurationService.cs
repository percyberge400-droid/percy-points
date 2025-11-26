using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;

namespace Pos.Application.Services.ConfigurationService
{
    public class ConfigurationService(IConfigurationRepository configurationRepository,
        ICloudLogService cloudLogService
        )
        : IConfigurationService
    {

        private readonly IConfigurationRepository _configurationRepository = configurationRepository;
        private readonly ICloudLogService _cloudLogService = cloudLogService;

        public async Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto)
        {
            try
            {
                var isEnabled = await _configurationRepository.IsCloudSyncEnabledAsync(dto.PosId, dto.Environment!);

                return isEnabled;

            }
            catch (Exception ex)
            {
                // Build the exception log
                var logDto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                    .WithExceptionInfo(ex)
                    .WithDomainInfo("SecurityEncryption", "Decrypt", null);

                // Send to cloud log (async)
                await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { logDto }, dto.Environment!);

                // Return false after logging
                return false;
            }
        }
    }
}