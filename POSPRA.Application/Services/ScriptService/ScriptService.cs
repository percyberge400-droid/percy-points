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
                DateCreated = dto.DateCreated,
                DateModified = dto.DateModified,
                IsSynced = (int)InvoiceStatus.Synced,
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

            var records = dtoList.Select(dto =>
            {
                string message = dto.Message!;
                DateTime createdAtPk = DateTime.Now;
                DateTime createdAtUtc = createdAtPk.ToUniversalTime();

                if (!string.IsNullOrWhiteSpace(dto.Message) && dto.Message.Contains("==>"))
                {
                    var parts = dto.Message.Split("==>", 2, StringSplitOptions.TrimEntries);

                    // ✅ Try parsing exact date format
                    string[] formats =
                    {
                        "M/d/yyyy h:mm:ss tt",
                        "MM/dd/yyyy hh:mm:ss tt",
                        "M/d/yyyy hh:mm:ss tt",
                        "MM/dd/yyyy h:mm:ss tt"
                    };

                    if (DateTime.TryParseExact(parts[0], formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsedDate))
                    {
                        createdAtPk = parsedDate;
                        createdAtUtc = parsedDate.ToUniversalTime();
                        message = parts[1]; // message without date
                    }
                }

                return new Logs
                {
                    POSID = posId,
                    Message = message,
                    CreatedAtPk = createdAtPk,
                    CreatedAtUtc = createdAtUtc,
                    IsSynced = true
                };
            }).ToList();

            await _logSQLiteRepository.AddRangeAsync(records);
            await _sqliteUnitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
