using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSPRA.Domain.Entities
{
    [Table("Invoice")]
    public class Invoice
    {
        [Key]
        public long InvoiceID { get; set; }

        [Required, StringLength(30)]
        public string FBRInvoiceNumber { get; set; } = null!;

        [Required]
        public long POSID { get; set; }

        [Required, StringLength(50)]
        public string USIN { get; set; } = null!;

        [Required]
        public DateTime DateTime { get; set; }

        [StringLength(150)]
        public string? BuyerName { get; set; }

        [StringLength(20)]
        public string? BuyerPhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalSaleValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalTaxCharged { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalBillAmount { get; set; }

        [Required]
        public int PaymentMode { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public byte? RequestFrom { get; set; }

        public byte? InvoiceType { get; set; }

        [StringLength(50)]
        public string? RefUSIN { get; set; }

        public bool? IsValidCalculation { get; set; }

        public bool? IsImport { get; set; }

        [StringLength(20)]
        public string? Version { get; set; }

        [StringLength(9)]
        public string? BuyerNTN { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FurtherTax { get; set; }

        [StringLength(13)]
        public string? BuyerCNIC { get; set; }

        public short? StatusID { get; set; }

        [StringLength(4000)]
        public string? Remarks { get; set; }
    }
}