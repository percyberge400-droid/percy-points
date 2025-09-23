using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSPRA.Domain.Entities
{
    public class ProductCatalogue
    {
        [Key]
        [Required]
        [MaxLength(100)]
        [Column("Product Code")]
        public string? ProductCode { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Product Description")]
        public string? ProductDescription { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("HS Code")]
        public string? HSCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Sale Type")]
        public string? SaleType { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("POS Unit of measurement")]
        public string? PosUnitOfMeasurement { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Tax Rate (%)")]
        public string? TaxRate { get; set; }

        [MaxLength(100)]
        [Column("SRO / Schedule NO")]
        public string? SroScheduleNumber { get; set; }

        [MaxLength(50)]
        [Column("Item Sr No")]
        public string? ItemSerialNumber { get; set; }
    }
}
