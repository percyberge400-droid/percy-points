using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;
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
        public ClientService(IClientRepository clientRepository, IMapper mapper, ISqlServerUnitOfWork sqlServerUnitOfWork, INetworkService networkService,
            IOptions<AppSettings> options)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _networkService = networkService;
            _settings = options.Value;
        }

        public async Task<ApiResponse<PosClients>> GetByMacAsync(ClientValidationDto dto)
        {
            // Check POS ID
            var entity = await _clientRepository.FirstOrDefaultAsync(m =>
                                m.POSRegistrationNumber == dto.PosId);

            if (entity == null)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.InvalidPosId,
                    null!,
                    string.Empty);
            }

            // Collect error messages for MAC and Token
            var errors = new List<string>();

            if (entity.MAC_Address != dto.MacAddress)
                errors.Add(ResponseMessages.InvalidMacAddress);

            if (entity.Token != dto.Token)
                errors.Add(ResponseMessages.InvalidToken);

            // If any error exists, return proper message
            if (errors.Any())
            {
                // If both are invalid → concatenate
                var errorMessage = string.Join(" | ", errors);

                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    errorMessage,
                    null!,
                    string.Empty);
            }

            if (entity.IsConfigured == true)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.AlreadyConfigured,
                    null!,
                    string.Empty);
            }

            var statusCode = await UpdateConfigurationFlag(true);

            if (statusCode == ApiStatusCode.ServiceUnavailable)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.ServiceUnavailable,
                    ResponseMessages.InternetNotAvailable,
                    null!,
                    string.Empty);
            }

            if (statusCode == ApiStatusCode.NotFound)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);
            }

            entity.IsConfigured = true;
            // ✅ All validations passed
            return new ApiResponse<PosClients>(
                ApiStatusCode.Success,
                ResponseMessages.RecordFound,
                entity,
                string.Empty);
        }

        public async Task<string> UpdateConfigurationFlag(bool isConfiguration)
        {
            bool internetAvailable = await _networkService.IsInternetAvailableAsync();

            if (!internetAvailable)
            {
                return ApiStatusCode.ServiceUnavailable;
            }

            var entity = await _clientRepository.FirstOrDefaultAsync(m =>
                            m.POSRegistrationNumber == _settings.POS);

            if (entity is null)
            {
                return ApiStatusCode.NotFound;
            }

            entity.IsConnected = isConfiguration;

            await _clientRepository.UpdateAsync(entity);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            return ApiStatusCode.Success;
        }

    }
}