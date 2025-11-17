namespace Pos.Application.DTOs.PageResponseDTOs
{
    public class PageResponseDto<T>
    {
        public List<T>? Items { get; set; }
        public int? TotalRecords { get; set; }
        public int? TotalPages { get; set; }
        public int? TotalSyncedInvoices { get; set; }
        public int? UnsyncedInvoices { get; set; }
    }
}
