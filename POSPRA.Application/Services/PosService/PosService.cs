using AutoMapper;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.PosDTOs;
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
                    message: ResponseMessages.ErrorUpdatingHeartbeat,
                    data: string.Empty
                );
            }
        }

        public async Task<ApiResponse<List<ResponseConfigurationDto>>> GetConfigurationsAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var posConfiguration = await _posConfigurationRepository.FirstOrDefaultAsync(x => x.POSID == posId && x.IsActive == true);
                if (posConfiguration == null)
                    return new ApiResponse<List<ResponseConfigurationDto>>(
                        statusCode: ApiStatusCodes.NotFound,
                        message: ResponseMessages.ConfigurationsNotFound,
                        data: null
                    );

                posConfiguration.IsActive = false;
                await _sqlServerUnitOfWork.SaveChangesAsync();

                // Auto-map dictionaries to your DTOs
                var results = _mapper.Map<List<ResponseConfigurationDto>>(posConfiguration);

                return new ApiResponse<List<ResponseConfigurationDto>>(
                    statusCode: ApiStatusCodes.Success,
                    message: ResponseMessages.ConfigurationsFound,
                    data: results
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ResponseConfigurationDto>>(
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
                PosStatus posStatus = new();
                await _posStatusRepository.AddAsync(posStatus);
                await _sqlServerUnitOfWork.SaveChangesAsync();

                // Auto-map dictionaries to your DTOs
                return new ApiResponse<List<PosStatus>>(
                    statusCode: ApiStatusCodes.Success,
                    message: ResponseMessages.ConfigurationsFound,
                    data: null
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PosStatus>>(
                    statusCode: ApiStatusCodes.NotFound,
                    message: $"{ResponseMessages.ConfigurationsFetchError} {ex.Message}",
                    data: null
                );
            }
        }
    }
}
