using AutoMapper;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.Interfaces;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ClientService
{
    public class ClientService : IClientService
    {
        private readonly IRepository<PosClients> _sqlClientRepository;
        private readonly IMapper _mapper;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly AppSettings _settings;
        public ClientService(
            ISqlServerRepositoryFactory sqlRepositoryFactory,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            IOptions<AppSettings> options)
        {
            _sqlClientRepository = sqlRepositoryFactory.CreateRepository<PosClients>();

            _mapper = mapper;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _settings = options.Value;
        }

        public async Task<ApiResponse<PosClients>> GetByMacAsync(ClientValidationDto dto)
        {
            // Check POS ID
            var entity = await _sqlClientRepository.FirstOrDefaultAsync(m =>
                                m.POSRegistrationNumber == dto.PosId);

            if (entity == null)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.InvalidPosId,
                    null!,
                    string.Empty);
            }

            //// ✅ 5. Validate MAC and Token
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

            // //✅ 6.Check configuration status
            if (entity.IsActive == true)
            {
                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.AlreadyConfigured,
                    null!,
                    string.Empty);
            }

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

        public async Task<string> UpdateConfigurationFlag(bool isConfiguration, long? posId)
        {
            // ✅ Use _settings.PosId if posId is null, 0, or not provided
            long effectivePosId = (posId.HasValue && posId.Value > 0)
                ? posId.Value
                : _settings.POS;

            var entity = await _sqlClientRepository.FirstOrDefaultAsync(m =>
                            m.POSRegistrationNumber == effectivePosId);

            if (entity is null)
            {
                return ApiStatusCode.NotFound;
            }

            entity.IsActive = isConfiguration;

            await _sqlClientRepository.UpdateAsync(entity);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            return ApiStatusCode.Success;
        }

        public async Task<bool> IsServiceEnabled(long posId)
        {
            var client = await _sqlClientRepository.FirstOrDefaultAsync(x => x.POSRegistrationNumber == posId);
            return client?.IsServiceEnabled ?? false;
        }
    }
}