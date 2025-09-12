using System.Data;
using Microsoft.Data.SqlClient;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Application.Services.POSService
{
    /// <summary>
    /// Service to handle POS-related operations such as updating heartbeat status.
    /// </summary>
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<object> _sqlServerRepository;

        /// <summary>
        /// Initializes a new instance of <see cref="PosService"/>.
        /// </summary>
        /// <param name="sqlServerRepository">Generic SQL Server repository for executing commands.</param>
        /// <param name="requestHeaderService">Service to retrieve request header info such as POS ID.</param>
        public PosService(
            SqlServerRepository<object> sqlServerRepository,
            IRequestHeaderService requestHeaderService)
        {
            _sqlServerRepository = sqlServerRepository;
            _requestHeaderService = requestHeaderService;
        }

        /// <summary>
        /// Updates the heartbeat of the current POS by executing the stored procedure
        /// <c>sp_UpdatePOSHeartbeat</c> and returns the result.
        /// </summary>
        /// <returns>An <see cref="ApiResponse{T}"/> containing the result status.</returns>
        public async Task<ApiResponse<string>> UpdateHeartBeatAsync()
        {
            try
            {
                // Retrieve the POS ID from the request header
                var posId = _requestHeaderService.GetPosId();

                // Prepare SQL parameters
                var inputParam = new SqlParameter("@POSID", SqlDbType.BigInt) { Value = posId };
                var outputParam = new SqlParameter("@Result", SqlDbType.Char, 1)
                {
                    Direction = ParameterDirection.Output
                };

                // Execute stored procedure with output parameter
                string? result = await _sqlServerRepository.ExecuteScalarProcedureWithOutputAsync(
                    "sp_UpdatePOSHeartbeat",
                    new[] { inputParam },
                    outputParam
                );

                return new ApiResponse<string>(
                    statusCode: "200",
                    message: "Heartbeat updated",
                    data: result
                );
            }
            catch (Exception ex)
            {
                // Log the exception if logging is configured
                // _logger.LogError(ex, "Error updating POS heartbeat");

                return new ApiResponse<string>(
                    statusCode: "500",
                    message: "Error updating heartbeat",
                    data: null
                );
            }
        }
    }
}
