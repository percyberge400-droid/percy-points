namespace Pos.Application.DTOs.ProductCatalogDtos
{
    public class ProductCatalogueQueryDto
    {
        public string? HSCode { get; set; }
        public string? ProductDescription { get; set; }
        public required int numberOfRecords { get; set; }
        public required int pageNumber { get; set; }
    }
}
