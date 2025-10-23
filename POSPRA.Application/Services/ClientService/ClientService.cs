using AutoMapper;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;
using POSPRA.Repositories.ClientRepository;

namespace POSPRA.Application.Services.ClientService
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        public ClientService(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
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

            // ✅ All validations passed
            return new ApiResponse<PosClients>(
                ApiStatusCode.Success,
                ResponseMessages.RecordFound,
                entity,
                string.Empty);
        }


    }
}