using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.DTOs.ConfigurationsDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;

namespace Pos.Application.Services.ConfigurationService
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ICloudLogService _cloudLogService;


        public ConfigurationService(
            IConfigurationRepository configurationRepository,
            ICloudLogService cloudLogService)
        {
            _configurationRepository = configurationRepository;
            _cloudLogService = cloudLogService;
        }

        public async Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto)
        {
            try
            {
                var isEnabled = await _configurationRepository.IsCloudSyncEnabledAsync(dto.PosId, dto.Environment!);
                return isEnabled;
            }
            catch (Exception ex)
            {
                var logDto = SyncLogBuilder.Build(AlertType.Exception, ex.Message, posId: 0)
                    .WithExceptionInfo(ex)
                    .WithDomainInfo("SecurityEncryption", "Decrypt", null);

                await _cloudLogService.CreateCloudLog(new List<SyncLogDto> { logDto }, dto.Environment!);
                return false;
            }
        }

        public async Task<ApiResponse<ConfigurationResponseDto>> GetUpdateVersion(string wwwrootPath, GetByModuleDto dto)
        {
            try
            {
                if (!Directory.Exists(wwwrootPath))
                    return new ApiResponse<ConfigurationResponseDto>(
                        ApiStatusCode.Error,
                        "wwwroot folder not found",
                        null!,
                        $"Expected wwwroot path: {wwwrootPath}"
                    );

                // Map the request to a valid folder name
                string? folderPath = dto.ModuleName switch
                {
                    ModuelNames.PRAPOS => ModuelNames.PRAPOS,
                    ModuelNames.DI => ModuelNames.DI,
                    ModuelNames.PRAPOS_WINDOW7 => ModuelNames.PRAPOS_WINDOW7,
                    _ => null
                };

                if (folderPath == null)
                    return new ApiResponse<ConfigurationResponseDto>(
                        ApiStatusCode.Error,
                        string.Empty,
                        null!,
                        "Unknown module requested."
                    );

                // Read version file safely
                string fullPath = Path.Combine(wwwrootPath, folderPath, FileNames.AppVersionFileName);
                string appVersion = await File.ReadAllTextAsync(fullPath).ContinueWith(t => t.Result.Trim(), TaskContinuationOptions.OnlyOnRanToCompletion);

                var configuration = new ConfigurationResponseDto { AppVersion = appVersion };

                return new ApiResponse<ConfigurationResponseDto>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordFound,
                    configuration,
                    string.Empty
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfigurationResponseDto>(
                    ApiStatusCode.Error,
                    ex.Message,
                    null!,
                    "Failed while reading version files."
                );
            }
        }

        public async Task<ApiResponse<ConfigurationZipFileResponseDto>> GetZipFileAsync(string wwwrootPath, GetByModuleDto dto)
        {
            try
            {
                // Map the request to a valid folder name
                string? folderPath = dto.ModuleName switch
                {
                    ModuelNames.PRAPOS => ModuelNames.PRAPOS,
                    ModuelNames.DI => ModuelNames.DI,
                    ModuelNames.PRAPOS_WINDOW7 => ModuelNames.PRAPOS_WINDOW7,
                    _ => null
                };

                if (folderPath == null)
                    return new ApiResponse<ConfigurationZipFileResponseDto>(
                        ApiStatusCode.Error,
                        string.Empty,
                        null,
                        "Unknown module requested."
                    );

                string fullPath = Path.Combine(wwwrootPath, folderPath, FileNames.UpdaterZipFileName);

                if (!File.Exists(fullPath))
                    return new ApiResponse<ConfigurationZipFileResponseDto>(
                        ApiStatusCode.Error,
                        string.Empty,
                        null,
                        "Zip file not found"
                    );

                // Read and convert the file to Base64
                byte[] fileBytes = await File.ReadAllBytesAsync(fullPath);
                string base64String = Convert.ToBase64String(fileBytes);

                return new ApiResponse<ConfigurationZipFileResponseDto>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordFound,
                    new ConfigurationZipFileResponseDto
                    {
                        FileName = FileNames.UpdaterZipFileName,
                        Base64File = base64String
                    },
                    string.Empty
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfigurationZipFileResponseDto>(
                    ApiStatusCode.Error,
                    ex.Message,
                    null,
                    "Failed while reading zip file."
                );
            }
        }
    }
}