using POSPRA.DTOs.FiscalDtos;

namespace POSPRA.DTOs.LogDTOs
{
    public class ScriptDTO
    {
        public List<SyncLogDto>? Log { get; set; }
        public List<FileRecordDto>? FileRecord { get; set; }
    }
}
