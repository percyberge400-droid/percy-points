using AutoMapper;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.PosDtos;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.PosRepository;
using POSPRA.Repositories.UnitOfWork;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<object> _sqlServerRepository;
        private readonly SqlServerRepository<PosConfiguration> _posConfigurationRepository;
        private readonly SqlServerRepository<PosStatus> _posStatusRepository;

        private readonly IPosClientRepository _posClientRepository;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        public readonly IMapper _mapper;
        public PosService(
            SqlServerRepository<object> sqlServerRepository,
            IRequestHeaderService requestHeaderService,
            IMapper mapper
,
            IPosClientRepository posClientRepository,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            SqlServerRepository<PosConfiguration> posConfigurationRepository,
            SqlServerRepository<PosStatus> posStatusRepository)
        {
            _sqlServerRepository = sqlServerRepository;
            _requestHeaderService = requestHeaderService;
            _mapper = mapper;
            _posClientRepository = posClientRepository;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _posConfigurationRepository = posConfigurationRepository;
            _posStatusRepository = posStatusRepository;
        }

        public async Task<ApiResponse<string>> UpdateHeartBeatAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var client = await _posClientRepository.FirstOrDefaultAsync(p => p.POSRegistrationNumber == posId);
                if (client == null)
                    return new ApiResponse<string>(
                        statusCode: ApiStatusCodes.NotFound,
                        message: ResponseMessages.DataNotFound,
                        data: string.Empty
                    );

                client.IsConnected = true;
                client.HeartbeatUpdatedOn = DateTime.Now;
                client.StoreStatus = "Connected";

                await _sqlServerUnitOfWork.SaveChangesAsync();

                return new ApiResponse<string>(
                    statusCode: ApiStatusCodes.Success,
                    message: ResponseMessages.HeartbeatUpdated,
                    data: string.Empty
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(
                    statusCode: ApiStatusCodes.Error,
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
                        statusCode: ApiStatusCodes.NotFound,
                        message: ResponseMessages.ConfigurationsNotFound,
                        data: null
                    );

                posConfigurations.IsActive = false;
                await _sqlServerUnitOfWork.SaveChangesAsync();

                return new ApiResponse<List<PosConfigurationDto>>(
                    statusCode: ApiStatusCodes.Success,
                    message: ResponseMessages.RecordSaved,
                    data: null
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PosConfigurationDto>>(
                    statusCode: ApiStatusCodes.NotFound,
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
                    statusCode: ApiStatusCodes.Success,
                    message: ResponseMessages.ConfigurationsFound,
                    data: new List<PosStatus> { posStatus }
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PosStatus>>(
                    statusCode: ApiStatusCodes.Error,
                    message: $"{ResponseMessages.ConfigurationsFetchError} {ex.Message}",
                    data: null
                );
            }
        }
    }
}
