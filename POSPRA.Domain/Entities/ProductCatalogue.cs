using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSPRA.Domain.Entities
{
    public class ProductCatalogue
    {
        [Key]
        [Required]
        [MaxLength(100)]
        [Column("ProductCode")]
        public long? ProductCode { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("ProductDescription")]
        public string? ProductDescription { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("HSCode")]
        public string? HSCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("SaleType")]
        public string? SaleType { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("POSUnitOfMeasurement")]
        public string? PosUnitOfMeasurement { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("TaxRate(%)")]
        public string? TaxRate { get; set; }

        [MaxLength(100)]
        [Column("SRO/ScheduleNO")]
        public string? SroScheduleNumber { get; set; }

        [MaxLength(50)]
        [Column("ItemSrNo")]
        public string? ItemSerialNumber { get; set; }
    }
}
