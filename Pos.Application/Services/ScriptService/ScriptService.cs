using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ScriptService
{
    public class ScriptService : IScriptService
    {
        private readonly ISqliteDynamicFactory _dynamicFactory;
        private readonly AppSettings _settings;

        public ScriptService(ISqliteDynamicFactory dynamicFactory,
            IOptions<AppSettings> options
            )
        {
            _dynamicFactory = dynamicFactory;
            _settings = options.Value;

        }

        public async Task<ApiResponse<ScriptDTO>> CreateScript(ScriptDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NewDbPath))
                    return new ApiResponse<ScriptDTO>(ApiStatusCode.Error, "NewDbPath is required", null!, "");

                // -----------------------------
                // Get SQLite password from AppSettings
                // -----------------------------
                if (string.IsNullOrWhiteSpace(_settings.Password))
                    return new ApiResponse<ScriptDTO>(ApiStatusCode.Error, "SQLite password is not configured", null!, "");

                // -----------------------------
                // Create new encrypted SQLite instance
                // -----------------------------
                var (fileRepo, fileUow) = _dynamicFactory.Create<FileRecord>(dto.NewDbPath, _settings.Password);
                var (logRepo, logUow) = _dynamicFactory.Create<Logs>(dto.NewDbPath, _settings.Password);

                int posId = 0;

                // -----------------------------
                // Insert File Records
                // -----------------------------
                if (dto.FileRecord?.Any() == true)
                {
                    await CreateScriptFileRecord(dto.FileRecord, fileRepo, fileUow);
                    posId = dto.FileRecord[0].POSID;
                }

                // -----------------------------
                // Insert Logs
                // -----------------------------
                if (dto.Log?.Any() == true && posId > 0)
                {
                    await CreateScriptLog(dto.Log, logRepo, logUow, posId);
                }
            }
            catch
            {
                return new ApiResponse<ScriptDTO>(ApiStatusCode.Error, ResponseMessages.DatabaseError, null!, "");
            }

            return new ApiResponse<ScriptDTO>(ApiStatusCode.Success, ResponseMessages.RecordSaved, null!, "");
        }

        private async Task<bool> CreateScriptFileRecord(
            List<FileRecordDto> dtoList,
            IRepository<FileRecord> repo,
            IUnitOfWork uow)
        {
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

            await repo.AddRangeAsync(records);
            await uow.SaveChangesAsync();

            return true;
        }

        private async Task<bool> CreateScriptLog(
            List<SyncLogDto> dtoList,
            IRepository<Logs> repo,
            IUnitOfWork uow,
            long posId)
        {
            var records = dtoList.Select(dto =>
            {
                string message = dto.Message!;
                DateTime createdAtPk = DateTime.Now;
                DateTime createdAtUtc = createdAtPk.ToUniversalTime();

                if (!string.IsNullOrWhiteSpace(dto.Message) && dto.Message.Contains("==>"))
                {
                    var parts = dto.Message.Split("==>", 2, StringSplitOptions.TrimEntries);

                    if (DateTime.TryParse(parts[0], out var parsedDate))
                    {
                        createdAtPk = parsedDate;
                        createdAtUtc = parsedDate.ToUniversalTime();
                        message = parts[1];
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

            await repo.AddRangeAsync(records);
            await uow.SaveChangesAsync();

            return true;
        }
    }
}