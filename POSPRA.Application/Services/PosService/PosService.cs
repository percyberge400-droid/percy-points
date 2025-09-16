using System.Data;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.PosDTOs;
using POSPRA.Repositories.BaseRepository;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.POSService
{
    public class PosService : IPosService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<object> _sqlServerRepository;
        public readonly IMapper _mapper;
        public PosService(
            SqlServerRepository<object> sqlServerRepository,
            IRequestHeaderService requestHeaderService,
            IMapper mapper
            )
        {
            _sqlServerRepository = sqlServerRepository;
            _requestHeaderService = requestHeaderService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<string>> UpdateHeartBeatAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();

                // Input parameter
                var inputParam = new SqlParameter("@POSID", SqlDbType.BigInt)
                {
                    Value = posId
                };

                // Output parameter
                var outputParam = new SqlParameter("@Result", SqlDbType.Char, 1)
                {
                    Direction = ParameterDirection.Output
                };

                // Use the generic helper to execute the procedure
                // We don't need row mapping here because we only care about the output parameter
                await _sqlServerRepository.ExecuteProcedureAsync<object>(
                    StoredProcedures.sp_UpdatePOSHeartbeat,
                    map: _ => default!,                            // no rows to map
                    parameters: new[] { inputParam, outputParam }
                );

                // Retrieve the output parameter value
                string? result = outputParam.Value?.ToString();

                return new ApiResponse<string>(
                    statusCode: ApiStatusCodes.Success,
                    message: ResponseMessages.HeartbeatUpdated,
                    data: result
                );
            }
            catch (Exception ex)
            {
                // Optionally log ex here

                return new ApiResponse<string>(
                    statusCode: ApiStatusCodes.Error,
                    message: ResponseMessages.ErrorUpdatingHeartbeat,
                    data: null
                );
            }
        }

        public async Task<ApiResponse<List<ResponseConfigurationDto>>> GetConfigurationsAsync()
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();
                var inputParam = new SqlParameter("@POSID", SqlDbType.BigInt)
                {
                    Value = posId
                };

                // Use the unified procedure executor.
                // We want List<Dictionary<string, object?>> so we set T accordingly
                // and skip the mapping delegate.
                var rawResults = await _sqlServerRepository.ExecuteProcedureAsync<Dictionary<string, object?>>(
                    StoredProcedures.sp_GetConfigurations,
                    parameters: new[] { inputParam }      // no mapper needed
                );

                // Auto-map dictionaries to your DTOs
                var results = _mapper.Map<List<ResponseConfigurationDto>>(rawResults);

                return new ApiResponse<List<ResponseConfigurationDto>>(
                    statusCode: ApiStatusCodes.Success,
                    message: results.Count > 0
                        ? ResponseMessages.ConfigurationsFound
                        : ResponseMessages.ConfigurationsNotFound,
                    data: results
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ResponseConfigurationDto>>(
                    statusCode: ApiStatusCodes.NotFound,
                    message: $"{ResponseMessages.ConfigurationsFetchError} {ex.Message}",
                    data: null
                );
            }
        }

        public async Task<string> InsertPosStatusAsync(IList<Logs> logs)
        {
            string response = string.Empty;

            try
            {
                var posId = _requestHeaderService.GetPosId();

                // Convert logs list to JSON (because the proc expects NVARCHAR(MAX) JSON)
                string logsJson = JsonConvert.SerializeObject(logs);

                var inputParams = new[]
                {
                    new SqlParameter("@POSID", SqlDbType.BigInt) { Value = posId },
                    new SqlParameter("@LogsJson", SqlDbType.NVarChar)
                    {
                        Value = logsJson
                    }
                };

                var outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };

                // Execute the procedure (no result set expected)
                await _sqlServerRepository.ExecuteProcedureAsync<object>(
                    "sp_InsertPOSStatus",
                    map: null,
                    parameters: inputParams.Concat(new[] { outputParam }).ToArray()
                );

                response = outputParam.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }

            return response;
        }

    }
}
