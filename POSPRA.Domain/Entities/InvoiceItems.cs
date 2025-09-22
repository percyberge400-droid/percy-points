using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSPRA.Domain.Entities
{
    [Table("InvoiceItemNew")]
    public class InvoiceItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long InvoiceItemId { get; set; }   // Primary Key

        public long InvoiceID { get; set; }       // Foreign Key -> Invoice

        [MaxLength(50)]
        public string? HSCode { get; set; }

        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [MaxLength(300)]
        public string? ProductDescription { get; set; }

        public decimal Rate { get; set; }
        public int UoM { get; set; }
        public decimal Quantity { get; set; }
        public decimal ValueSalesExcludingST { get; set; }
        public decimal SalesTaxApplicable { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? STWithheldAtSource { get; set; }
        public decimal? ExtraTax { get; set; }
        public decimal? FurtherTax { get; set; }
        public int? SroScheduleNo { get; set; }
        public decimal? FedPayable { get; set; }
        public decimal? CVT { get; set; }
        public decimal? WHIT_1 { get; set; }
        public decimal? WHIT_2 { get; set; }

        [MaxLength(50)]
        public string? WHIT_Section_1 { get; set; }

        [MaxLength(50)]
        public string? WHIT_Section_2 { get; set; }

        public decimal TotalValues { get; set; }

        // Common fields
        public bool IsActive { get; set; } = true;
        public DateTime EntryDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Invoice Invoice { get; set; } = null!;
    }
}
