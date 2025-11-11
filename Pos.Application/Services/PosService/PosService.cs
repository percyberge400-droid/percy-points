using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.PosDtos;
using Pos.Application.Interfaces;
using Pos.Application.Services.HelperService;
using Pos.Application.Services.PosService;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly IRepository<PosClients> _sqlClientRepository;
        private readonly IRepository<POSConfigurations> _sqlConfigurationsRepository;
        private readonly IRepository<PosStatus> _sqlStatusRepository;


        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        public readonly IMapper _mapper;
        public PosService(
            ISqlServerRepositoryFactory sqlRepositoryFactory,
            IRequestHeaderService requestHeaderService,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork)
        {

            _sqlClientRepository = sqlRepositoryFactory.CreateRepository<PosClients>();
            _sqlConfigurationsRepository = sqlRepositoryFactory.CreateRepository<POSConfigurations>();
            _sqlStatusRepository = sqlRepositoryFactory.CreateRepository<PosStatus>();
            _requestHeaderService = requestHeaderService;
            _mapper = mapper;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
        }

        public async Task<ApiResponse<HeartBeatDto>> UpdateHeartBeatAsync(int posId)
        {
            try
            {
                var client = await _sqlClientRepository.FirstOrDefaultAsync(p => p.POSRegistrationNumber == posId);
                if (client == null)
                    return new ApiResponse<HeartBeatDto>(
                        statusCode: ApiStatusCode.NotFound,
                        message: ResponseMessages.DataNotFound,
                        data: null
                    );

                client.IsConnected = true;
                client.HeartbeatUpdatedOn = DateTime.Now;
                client.StoreStatus = "Connected";

                await _sqlServerUnitOfWork.SaveChangesAsync();

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

        public async Task<ApiResponse<List<PosConfigurationDto>>> GetConfigurationsAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var posConfigurations = await _sqlConfigurationsRepository.FirstOrDefaultAsync(x => x.POSID == posId && x.IsActive == true);
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

                await _sqlStatusRepository.AddAsync(posStatus);
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
