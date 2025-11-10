using Pos.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("InvoiceItems")]
public class InvoiceItems
{
    [Key]
    public long InvoiceItemId { get; set; }

    [Required]
    [ForeignKey("Invoice")]
    public long InvoiceID { get; set; }

    [Required, StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PCTCode { get; set; }

    public decimal? Quantity { get; set; }

    public double TaxRate { get; set; }

    public decimal? SaleValue { get; set; }

    public decimal? TaxCharged { get; set; }

    public decimal? TotalAmount { get; set; }

    public bool IsActive { get; set; }

    public DateTime EntryDate { get; set; }

    public byte? InvoiceType { get; set; }

    [StringLength(50)]
    public string? RefUSIN { get; set; }

    public decimal? Discount { get; set; }

    public decimal? FurtherTax { get; set; }

    public Invoice? Invoice { get; set; }
}