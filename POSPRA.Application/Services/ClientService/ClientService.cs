using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.ClientRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.ClientService
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly INetworkService _networkService;
        private readonly AppSettings _settings;
        private readonly IConfiguration _configuration;
        public ClientService(IClientRepository clientRepository, IMapper mapper, ISqlServerUnitOfWork sqlServerUnitOfWork, INetworkService networkService,
            IOptions<AppSettings> options, IConfiguration configuration)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _networkService = networkService;
            _settings = options.Value;
            _configuration = configuration;
        }

        public async Task<ApiResponse<PosClients>> GetByMacAsync(ClientValidationDto dto)
        {
            try
            {
                // ✅ 1. Select environment
                string environment = dto.Environment?.Trim().ToLower() ?? "sandbox";

                // ✅ 2. Choose connection string from configuration
                string connectionStringKey = environment == "production"
                    ? "SqlServerConnectionProduction"
                    : "SqlServerConnectionSandbox";

                string? connectionString = _configuration.GetConnectionString(connectionStringKey);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return new ApiResponse<PosClients>(
                        ApiStatusCode.NotFound,
                        $"Connection string not found for environment: {environment}",
                        null!,
                        string.Empty);
                }

                // ✅ 3. Create new DbContext with selected connection
                var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var sqlServerContext = new SqlServerDbContext(optionsBuilder.Options);

                // ✅ 4. Query PosClients table
                var entity = await sqlServerContext.Set<PosClients>()
                    .FirstOrDefaultAsync(m => m.POSRegistrationNumber == dto.PosId);

                if (entity == null)
                {
                    return new ApiResponse<PosClients>(
                        ApiStatusCode.NotFound,
                        ResponseMessages.InvalidPosId,
                        null!,
                        string.Empty);
                }

                // ✅ 5. Validate MAC and Token
                var errors = new List<string>();

                if (entity.MacAddressInput != dto.MacAddress)
                    errors.Add(ResponseMessages.InvalidMacAddress);

                if (entity.Token != dto.Token)
                    errors.Add(ResponseMessages.InvalidToken);

                if (errors.Any())
                {
                    var errorMessage = string.Join(" | ", errors);
                    return new ApiResponse<PosClients>(
                        ApiStatusCode.NotFound,
                        errorMessage,
                        null!,
                        string.Empty);
                }

                // ✅ 6. Check configuration status
                if (entity.IsActive == true)
                {
                    return new ApiResponse<PosClients>(
                        ApiStatusCode.NotFound,
                        ResponseMessages.AlreadyConfigured,
                        null!,
                        string.Empty);
                }

                // ✅ 7. Update flag (reuse your existing helper)
                var statusCode = await UpdateConfigurationFlag(true, dto.PosId);

                if (statusCode == ApiStatusCode.NotFound)
                {
                    return new ApiResponse<PosClients>(
                        ApiStatusCode.NotFound,
                        ResponseMessages.DataNotFound,
                        null!,
                        string.Empty);
                }

                entity.IsActive = true;

                // ✅ 8. Return success
                return new ApiResponse<PosClients>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordFound,
                    entity,
                    string.Empty);
            }
            catch (Exception ex)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.ServiceUnavailable,
                    $"Error: {ex.Message}",
                    null!,
                    string.Empty);
            }
        }

        public async Task<string> UpdateConfigurationFlag(bool isConfiguration, long? posId)
        {
            // ✅ Use _settings.PosId if posId is null, 0, or not provided
            long effectivePosId = (posId.HasValue && posId.Value > 0)
                ? posId.Value
                : _settings.POS;

            var entity = await _clientRepository.FirstOrDefaultAsync(m =>
                            m.POSRegistrationNumber == effectivePosId);

            if (entity is null)
            {
                return ApiStatusCode.NotFound;
            }

            entity.IsActive = isConfiguration;

            await _clientRepository.UpdateAsync(entity);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            return ApiStatusCode.Success;
        }

        public async Task<bool> IsServiceEnabled(long posId)
        {
            var client = await _clientRepository.FirstOrDefaultAsync(x => x.POSRegistrationNumber == posId);
            return client?.IsServiceEnabled ?? false;
        }
    }
}