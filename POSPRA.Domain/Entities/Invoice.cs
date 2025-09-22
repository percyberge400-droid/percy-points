using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSPRA.Domain.Entities
{
    [Table("InvoiceNew")]
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long InvoiceID { get; set; }   // Primary Key

        public long BPOSID { get; set; }      // bigint
        public int? InvoiceType { get; set; }  // int
        public DateTime? InvoiceDate { get; set; }

        [MaxLength(50)]
        public string? NTN_CNIC { get; set; }

        [MaxLength(200)]
        public string? BuyerSellerName { get; set; }

        [MaxLength(300)]
        public string? DestinationAddress { get; set; }

        public int? SaleType { get; set; }

        public decimal? TotalSalesTaxApplicable { get; set; }
        public decimal TotalRetailPrice { get; set; }
        public decimal? TotalSTWithheldAtSource { get; set; }
        public decimal? TotalExtraTax { get; set; }
        public decimal? TotalFEDPayable { get; set; }
        public decimal? TotalWithheldIncomeTax { get; set; }
        public decimal? TotalCVT { get; set; }

        [MaxLength(50)]
        public string? Distributor_NTN_CNIC { get; set; }

        [MaxLength(200)]
        public string? DistributorName { get; set; }

        public bool IsActive { get; set; } = true;   // Common for entities
        public DateTime? EntryDate { get; set; } = DateTime.Now;
    }
}
