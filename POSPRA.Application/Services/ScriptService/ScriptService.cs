using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.LogDTOs;
using POSPRA.Repositories.FileRecordRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.ScriptService
{
    public class ScriptService : IScriptService
    {
        private readonly ILogSQLiteRepository _logSQLiteRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IFileRecordRepository _fileRecordRepository;

        public ScriptService(ILogSQLiteRepository logSQLiteRepository, ISqliteUnitOfWork sqliteUnitOfWork, IFileRecordRepository fileRecordRepository)
        {
            _logSQLiteRepository = logSQLiteRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _fileRecordRepository = fileRecordRepository;
        }

        public async Task<ApiResponse<ScriptDTO>> CreateScript(ScriptDTO dto)
        {
            try
            {
                int posId = 0;
                if (dto.FileRecord is not null && dto.FileRecord.Any())
                {
                    await CreateScriptFileRecord(dto.FileRecord);
                    posId = dto.FileRecord[0].POSID;
                }

                if (dto.Log is not null && dto.Log.Any() && posId > 0)
                {
                    await CreateScriptLog(dto.Log, posId);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<ScriptDTO>(ApiStatusCode.Error, ResponseMessages.DatabaseError, null!, string.Empty);
            }

            return new ApiResponse<ScriptDTO>(ApiStatusCode.Success, ResponseMessages.RecordSaved, null!, string.Empty);

        }
        private async Task<bool> CreateScriptFileRecord(List<FileRecordDto> dtoList)
        {
            if (dtoList == null || dtoList.Count == 0)
                return false;

            var records = dtoList.Select(dto => new FileRecord
            {
                POSID = dto.POSID,
                InvoiceNumber = dto.InvoiceNumber,
                InvoiceData = dto.InvoiceData,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                IsSynced = dto.IsSynced == 1 ? (int)InvoiceStatus.Synced : (int)InvoiceStatus.NotSynced,
                AttemptCount = 0
            }).ToList();

            await _fileRecordRepository.AddRangeAsync(records);
            await _sqliteUnitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task<bool> CreateScriptLog(List<SyncLogDto> dtoList, long posId)
        {
            if (dtoList == null || dtoList.Count == 0)
                return false;

            var records = dtoList.Select(dto => new Logs
            {
                POSID = posId,
                Message = dto.Message,
                IsSynced = dto.IsSynced
            }).ToList();

            await _logSQLiteRepository.AddRangeAsync(records);
            await _sqliteUnitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
