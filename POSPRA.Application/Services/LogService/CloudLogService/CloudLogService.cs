using AutoMapper;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.LogService
{
    public class CloudLogService : ICloudLogService
    {
        private readonly ILogSQLServerRepository _logSQLServerRepository;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly IMapper _mapper;

        public CloudLogService(
            ILogSQLServerRepository logSQLServerRepository,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            IMapper mapper)
        {
            _logSQLServerRepository = logSQLServerRepository;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<LogDto>>> GetAllCloudAsync()
        {
            var allRecords = await _logSQLServerRepository.GetAllAsync();
            var logDtos = _mapper.Map<List<LogDto>>(allRecords);

            if (logDtos.Any())
                return new ApiResponse<List<LogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<LogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null!, string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos)
        {
            if (logDtos == null || !logDtos.Any())
                return new ApiResponse<List<LogDto>>(ApiStatusCode.Error, ResponseMessages.DataNotFound, null!, string.Empty);

            var entities = _mapper.Map<List<Logs>>(logDtos);
            entities.ForEach(log => log.IsSynced = true);

            _logSQLServerRepository.UpdateRange(entities);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            var updatedDtos = _mapper.Map<List<LogDto>>(entities);
            return new ApiResponse<List<LogDto>>(ApiStatusCode.Success, ResponseMessages.RecordUpdated, updatedDtos, string.Empty);
        }

        public async Task<ApiResponse<List<SyncLogDto>>> CreateCloudLog(List<SyncLogDto> dto)
        {
            if (dto is null || !dto.Any())
                return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.Error, ResponseMessages.InvalidInput, null!, string.Empty);

            var logs = _mapper.Map<List<Logs>>(dto);
            logs.ForEach(l => l.Id = 0);

            await _logSQLServerRepository.AddRangeAsync(logs);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            dto.ForEach(x => x.IsSynced = true);
            return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.Success, ResponseMessages.RecordSaved, dto, string.Empty);
        }
    }
}
