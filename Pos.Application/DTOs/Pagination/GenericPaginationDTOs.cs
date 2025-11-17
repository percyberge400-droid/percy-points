namespace Pos.Application.DTOs.Pagination
{
    public class GenericPaginationDTOs
    {
        public required int PageNumber { get; set; }
        public required int NumberOfRecords { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
