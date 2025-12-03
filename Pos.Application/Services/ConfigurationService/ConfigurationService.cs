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

        public async Task<ApiResponse<ConfigurationResponseDto>> GetUpdateVersion()
        {
            try
            {
                // Get absolute wwwroot path (project folder + wwwroot)
                string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
                string wwwrootPath = Path.Combine(projectRoot, "Pos.Api", "wwwroot", "Configurations");

                if (!Directory.Exists(wwwrootPath))
                {
                    return new ApiResponse<ConfigurationResponseDto>(
                        ApiStatusCode.Error,
                        "wwwroot folder not found",
                        null,
                        $"Expected wwwroot path: {wwwrootPath}"
                    );
                }

                // Helper function to read files safely
                async Task<string> ReadVersionFileAsync(string fileName)
                {
                    string filePath = Path.Combine(wwwrootPath, fileName);
                    if (!File.Exists(filePath))
                        return "File Not Found";

                    return (await File.ReadAllTextAsync(filePath)).Trim();
                }

                //string launcherVersion = await ReadVersionFileAsync(FileName.LauncherVersionFile);
                string appVersion = await ReadVersionFileAsync(FileName.AppVersionFile);

                var configuration = new ConfigurationResponseDto
                {
                    LauncherVersion = null!,
                    AppVersion = appVersion
                };

                return new ApiResponse<ConfigurationResponseDto>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordFound,
                    configuration,
                    string.Empty);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfigurationResponseDto>(
                    ApiStatusCode.Error,
                    ex.Message,
                    null,
                    "Failed while reading version files.");
            }
        }
        public async Task<(string base64, string fileName)> GetZipFileAsync()
        {
            string fileName = FileName.UpdaterZipFile;

            // Build project root path (your existing logic)
            string projectRoot = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")
            );

            // Final file path
            string filePath = Path.Combine(projectRoot, "Pos.Api", "wwwroot", fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Zip file not found", filePath);

            // Read file bytes
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

            // Convert to Base64
            string base64String = Convert.ToBase64String(fileBytes);

            return (base64String, fileName);
        }

    }
}
