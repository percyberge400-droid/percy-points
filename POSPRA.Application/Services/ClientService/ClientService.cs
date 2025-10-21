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

            var entity = await _clientRepository.FirstOrDefaultAsync(m =>
                                m.POSRegistrationNumber == dto.PosId &&
                                m.MAC_Address == dto.MacAddress &&
                                m.Token == dto.Token);
            if (entity == null)
                return new ApiResponse<PosClients>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);

            return new ApiResponse<PosClients>(
                ApiStatusCode.Success,
                ResponseMessages.RecordFound,
                entity,
                string.Empty);
        }
    }
}