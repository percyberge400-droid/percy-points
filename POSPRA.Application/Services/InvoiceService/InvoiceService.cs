using System.Data;
using AutoMapper;
using Microsoft.Data.SqlClient; // ✅ correct namespace
using POSPRA.Application.Services.HelperService;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.Repositories.BaseRepository;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IRequestHeaderService _requestHeaderService;
        private readonly SqlServerRepository<object> _sqlServerRepository;
        public readonly IMapper _mapper;

        public InvoiceService(IRequestHeaderService requestHeaderService, SqlServerRepository<object> sqlServerRepository, IMapper mapper)
        {
            _requestHeaderService = requestHeaderService;
            _sqlServerRepository = sqlServerRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<POSVerificationDto>>> POS_VerificationAsync(string bodyPosIds)
        {
            try
            {
                var posId = _requestHeaderService.GetPosId();

                var parameters = new[]
                {
                    new SqlParameter("@POSID", SqlDbType.BigInt) { Value = posId },
                    new SqlParameter("@POSList", SqlDbType.NVarChar, -1) { Value = bodyPosIds }
                };

                // ✅ Generic executor: returns List<Dictionary<string, object?>>
                var rawResults = await _sqlServerRepository.ExecuteProcedureAsync<Dictionary<string, object?>>(
                    "SP_POSVerification",
                    parameters: parameters);

                // ✅ AutoMapper converts dictionary rows to POSVerificationDTO
                var dtoResults = _mapper.Map<List<POSVerificationDto>>(rawResults);

                return new ApiResponse<List<POSVerificationDto>>(
                    statusCode: ApiStatusCodes.Success,
                    message: dtoResults.Count > 0
                        ? "POS verification records found"
                        : "No records found",
                    data: dtoResults
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<POSVerificationDto>>(
                    statusCode: ApiStatusCodes.Error,
                    message: "Error fetching POS verification: " + ex.Message,
                    data: null
                );
            }
        }
    }
}