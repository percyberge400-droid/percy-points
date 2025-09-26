using AutoMapper;
using POSPRA.Application.Utility;
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

        public async Task<ApiResponse<bool>> GetByMacAsync(ClientValidationDto dto)
        {

            var entity = await _clientRepository.FirstOrDefaultAsync(m =>
                                m.POSBranchID == dto.PosId &&
                                m.MAC_Address == dto.MacAddress &&
                                m.Token == dto.Token);
            if (entity == null)
                return new ApiResponse<bool>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.DataNotFound,
                    false,
                    string.Empty);

            return new ApiResponse<bool>(
                ApiStatusCode.Success,
                ResponseMessages.RecordFound,
                true,
                string.Empty);
        }
    }
}