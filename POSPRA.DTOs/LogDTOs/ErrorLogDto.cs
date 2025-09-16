namespace POSPRA.DTOs.LogDTOs
{
    public class ErrorLogDto
    {
        public string Message { get; set; } = string.Empty;

        public int POSID { get; set; } = 0;

        public string ActualData { get; set; } = string.Empty;

        public bool IsValidSignature { get; set; } = false;

        public int TotalFiles { get; set; } = 0;
    }
}
