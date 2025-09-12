using Microsoft.Data.SqlClient;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Repositories.BaseRepository;
using System.Data;

namespace POSPRA.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<object> _sqlServerRepository;

        public PosService(SqlServerRepository<object> sqlServerRepository,
                          IRequestHeaderService requestHeaderService)
        {
            _sqlServerRepository = sqlServerRepository;
            _requestHeaderService = requestHeaderService;
        }

        public async Task<ApiResponse<string>> UpdateHeartBeatAsync()
        {
            try
            {
                //long posId = 110039;
                var posId = _requestHeaderService.GetPosId();
                // Input parameter
                var inputParam = new SqlParameter("@POSID", SqlDbType.BigInt) { Value = posId };

                // Output parameter
                var outputParam = new SqlParameter("@Result", SqlDbType.Char, 1)
                {
                    Direction = ParameterDirection.Output
                };

                // Execute the stored procedure
                string? result = await _sqlServerRepository.ExecuteScalarProcedureWithOutputAsync(
                    "sp_UpdatePOSHeartbeat",
                    new SqlParameter[] { inputParam },
                    outputParam
                );

                return new ApiResponse<string>(
                    statusCode: "200",
                    message: "Heartbeat updated",
                    data: result
                );
            }
            catch (Exception)
            {
                // Log ex if needed
                return new ApiResponse<string>(
                    statusCode: "500",
                    message: "Error updating heartbeat",
                    data: null
                );
            }
        }
    }
}
