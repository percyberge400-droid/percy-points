using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.PosDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.HelperService;
using Pos.Application.Services.PosService;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        //private readonly IRepository<PosClients> _sqlClientRepository;
        //private readonly IRepository<POSConfigurations> _sqlConfigurationsRepository;
        private readonly IPosClientRepository _posClientRepository;
        private readonly IConfigurationRepository _posConfigurationRepository;
        //private readonly IRepository<PosStatus> _sqlStatusRepository;


        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        public readonly IMapper _mapper;
        public PosService(
            //ISqlServerRepositoryFactory sqlRepositoryFactory,
            IPosClientRepository posClientRepository,
            IConfigurationRepository posConfigurationRepository,
            IRequestHeaderService requestHeaderService,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork)
        {
            _posClientRepository = posClientRepository;
            _posConfigurationRepository = posConfigurationRepository;
            // _sqlClientRepository = sqlRepositoryFactory.CreateRepository<PosClients>();
            //_sqlConfigurationsRepository = sqlRepositoryFactory.CreateRepository<POSConfigurations>();
            // _sqlStatusRepository = sqlRepositoryFactory.CreateRepository<PosStatus>();
            _requestHeaderService = requestHeaderService;
            _mapper = mapper;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
        }

        public async Task<ApiResponse<HeartBeatDto>> UpdateHeartBeatAsync(int posId, string env)
        {
            try
            {
                var client = _posClientRepository.UpdatePosClientHeartBeat(posId, env);

                var dto = _mapper.Map<HeartBeatDto>(client);

                return new ApiResponse<HeartBeatDto>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.HeartbeatUpdated,
                    data: dto
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<HeartBeatDto>(
                    statusCode: ApiStatusCode.Error,
                    message: $"{ResponseMessages.ErrorUpdatingHeartbeat}: {ex.Message}",
                    data: null
                );
            }

        }

        public async Task<ApiResponse<List<PosConfigurationDto>>> GetConfigurationsAsync(string env)
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var posConfigurations = await _posConfigurationRepository.UpdateConfigurationStatus(posId, env);
                if (posConfigurations == null)
                    return new ApiResponse<List<PosConfigurationDto>>(
                        statusCode: ApiStatusCode.NotFound,
                        message: ResponseMessages.ConfigurationsNotFound,
                        data: null
                    );

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

                //await _posClientRepository(posStatus);
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
