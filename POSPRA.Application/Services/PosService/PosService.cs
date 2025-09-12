using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.PosDTOs;
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
        //
        public async Task<ApiResponse<List<ResponseConfigurationDto>>> GetConfigurationsAsync()
        {
            try
            {
                long posId = 110039;
                // var posId = _requestHeaderService.GetPosId();

                var inputParam = new SqlParameter("@POSID", SqlDbType.BigInt)
                {
                    Value = posId
                };

                // Get raw dictionary list from repository
                var rawResults = await _sqlServerRepository.ExecuteProcedureToDictionaryListAsync(
                    "sp_GetConfigurations", inputParam);

                // Map dictionaries to DTOs
                var results = rawResults.Select(row => new ResponseConfigurationDto
                {
                    LogInterval = row["LogInterval"]?.ToString(),
                    RecordInterval = row["RecordInterval"]?.ToString(),
                    HeartbeatInterval = row["HeartbeatInterval"]?.ToString(),
                    IMSUpdateInterval = row["IMSUpdateInterval"]?.ToString(),
                    RecordSyncLimit = row["RecordSyncLimit"]?.ToString(),
                    LogSyncLimit = row["LogSyncLimit"]?.ToString(),
                    GatewayURL = row["GatewayURL"]?.ToString(),
                    FilePath = row["FilePath"]?.ToString(),
                    Version = row["Version"]?.ToString(),
                    FileSize = row["FileSize"]?.ToString(),
                    Token = row["Token"]?.ToString()
                }).ToList();

                return new ApiResponse<List<ResponseConfigurationDto>>(
                    statusCode: "200",
                    message: results.Count > 0 ? "Success...Configuration(s) found." : "No configuration(s) found.",
                    data: results
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ResponseConfigurationDto>>(
                    statusCode: "500",
                    message: $"Error occurred while fetching configurations: {ex.Message}",
                    data: null
                );
            }
        }


        public async Task<string> InsertPosStatusAsync(IList<Logs> logs)
        {
            //  Convert logs to DataTable using SharedResources
            var logsTable = SharedResources.ToDataTable(logs);
            // var posId = _requestHeaderService.GetPosId(); // or from headers
            long posId = 110039;
            if (logs == null || logs.Count == 0)
                return "No logs to insert";

            // Serialize logs list into JSON
            string logsJson = JsonConvert.SerializeObject(logs);

            //  Call repository SP
            string result = await _sqlServerRepository.InsertPOSStatusAsync(posId, logsJson);

            return result; // "Success" or "Error: ..."
        }
    }
}
