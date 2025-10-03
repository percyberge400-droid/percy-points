using AutoMapper;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.PosDtos;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.ClientRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<object> _sqlServerRepository;
        private readonly SqlServerRepository<POSConfigurations> _posConfigurationRepository;
        private readonly SqlServerRepository<PosStatus> _posStatusRepository;

        private readonly IClientRepository _clientRepository;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        public readonly IMapper _mapper;
        public PosService(
            SqlServerRepository<object> sqlServerRepository,
            IRequestHeaderService requestHeaderService,
            IMapper mapper
,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            SqlServerRepository<POSConfigurations> posConfigurationRepository,
            SqlServerRepository<PosStatus> posStatusRepository,
            IClientRepository clientRepository)
        {
            _sqlServerRepository = sqlServerRepository;
            _requestHeaderService = requestHeaderService;
            _mapper = mapper;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _posConfigurationRepository = posConfigurationRepository;
            _posStatusRepository = posStatusRepository;
            _clientRepository = clientRepository;
        }

        public async Task<ApiResponse<string>> UpdateHeartBeatAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var client = await _clientRepository.FirstOrDefaultAsync(p => p.POSRegistrationNumber == posId);
                if (client == null)
                    return new ApiResponse<string>(
                        statusCode: ApiStatusCode.NotFound,
                        message: ResponseMessages.DataNotFound,
                        data: string.Empty
                    );

                client.IsConnected = true;
                client.HeartbeatUpdatedOn = DateTime.Now;
                client.StoreStatus = "Connected";

                await _sqlServerUnitOfWork.SaveChangesAsync();

                return new ApiResponse<string>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.HeartbeatUpdated,
                    data: string.Empty
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(
                    statusCode: ApiStatusCode.Error,
                    message: $"{ResponseMessages.ErrorUpdatingHeartbeat}: {ex.Message}",
                    data: string.Empty
                );
            }

        }

        public async Task<ApiResponse<List<PosConfigurationDto>>> GetConfigurationsAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var posConfigurations = await _posConfigurationRepository.FirstOrDefaultAsync(x => x.POSID == posId && x.IsActive == true);
                if (posConfigurations == null)
                    return new ApiResponse<List<PosConfigurationDto>>(
                        statusCode: ApiStatusCode.NotFound,
                        message: ResponseMessages.ConfigurationsNotFound,
                        data: null
                    );

                posConfigurations.IsActive = false;
                await _sqlServerUnitOfWork.SaveChangesAsync();

                return new ApiResponse<List<PosConfigurationDto>>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordSaved,
                    data: null
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PosConfigurationDto>>(
                    statusCode: ApiStatusCode.NotFound,
                    message: $"{ResponseMessages.ConfigurationsFetchError} {ex.Message}",
                    data: null
                );
            }
        }

        public async Task<ApiResponse<List<PosStatus>>> InsertPosStatusAsync()
        {
            try
            {
                // Get POS-ID from request header (must match POSClients.POSRegistrationNumber)
                var posId = _requestHeaderService.GetPosId();

                // Create a new status record
                PosStatus posStatus = new()
                {
                    POSID = posId,                          // FK value
                    Message = "POS status inserted",        // Example message
                    DateCreated = DateTime.Now,             // Timestamp
                    TypeId = 1                              // Example type (adjust as needed)
                };

                await _posStatusRepository.AddAsync(posStatus);
                await _sqlServerUnitOfWork.SaveChangesAsync();

                return new ApiResponse<List<PosStatus>>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.ConfigurationsFound,
                    data: new List<PosStatus> { posStatus }
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PosStatus>>(
                    statusCode: ApiStatusCode.Error,
                    message: $"{ResponseMessages.ConfigurationsFetchError} {ex.Message}",
                    data: null
                );
            }
        }
    }
}
