using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.DTOs.LogDTOs
{
    public class ScriptDTO
    {
        public List<LogDto>? Log { get; set; }
        public List<FileRecordDto>? FileRecord { get; set; }
    }
}
