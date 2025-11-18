using Pos.Application.DTOs.FiscalDtos;

namespace Pos.Application.DTOs.LogDTOs
{
    public class ScriptDTO
    {
        public List<SyncLogDto>? Log { get; set; }
        public List<FileRecordDto>? FileRecord { get; set; }
        public string? NewDbPath { get; set; }
    }
}
