using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.DTOs.PosDTOs;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<ResponseHeartbeatDTO> _sqlServerRepository;

        public PosService(SqlServerRepository<ResponseHeartbeatDTO> sqlServerRepository, IRequestHeaderService requestHeaderService)
        {
            _sqlServerRepository = sqlServerRepository;
            _requestHeaderService = requestHeaderService;
        }

        public async Task<ApiResponse<string>> UpdateHeartBeatAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                // Stored procedure call with output parameter captured as a column
                // The procedure should SELECT the @Result at the end:
                //   SELECT @Result AS Result;
                var results = await _sqlServerRepository.QueryProcedureAsync<ResponseHeartbeatDTO>(
                    $"EXEC sp_UpdatePOSHeartbeat @POSID = {posId}");

                // If the proc SELECTs the value, we just read it
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
