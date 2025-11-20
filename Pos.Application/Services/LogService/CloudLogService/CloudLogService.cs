using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.LogService
{
    public class CloudLogService : ICloudLogService
    {
        private readonly IRepository<Logs> _sqlLogRepository;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly IMapper _mapper;

        public CloudLogService(
            ISqlServerRepositoryFactory sqlRepositoryFactory,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            IMapper mapper)
        {
            _sqlLogRepository = sqlRepositoryFactory.CreateRepository<Logs>();
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<LogDto>>> GetAllCloudAsync()
        {
            var allRecords = await _sqlLogRepository.GetAllAsync();
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

            _sqlLogRepository.UpdateRange(entities);
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

            await _sqlLogRepository.AddRangeAsync(logs);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            dto.ForEach(x => x.IsSynced = true);
            return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.Success, ResponseMessages.RecordSaved, dto, string.Empty);
        }
    }
}
