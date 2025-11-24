using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Domain.Entities
{
    [Table("POSMASTER")]
    public class POSMASTER
    {
        [Key]
        public long POSMASTERID { get; set; }

        public long? Computer_No { get; set; }

        [MaxLength(9)]
        public string? NTN { get; set; }

        [MaxLength(13)]
        public string? STRN { get; set; }

        [MaxLength(100)]
        public string? BusinessName { get; set; }

        [MaxLength(100)]
        public string? BrandName { get; set; }

        [MaxLength(100)]
        public string? Products { get; set; }

        public short? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? Website { get; set; }

        public long? AnnualTurnover { get; set; }

        public long? EstimatedTransDayWise { get; set; }

        public short? SWDevelopment { get; set; }

        [MaxLength(250)]
        public string? POSSWName { get; set; }

        [MaxLength(500)]
        public string? POSSWTechnologies { get; set; }

        public short? POSSWTechnologyType { get; set; }

        [MaxLength(250)]
        public string? POSSWVendor { get; set; }

        public int? TechstaffStrength { get; set; }

        public DateTime? EntryDate { get; set; }

        public bool? IsActive { get; set; }

        public short? IsLive { get; set; }

        public DateTime? LiveDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        [MaxLength(100)]
        public string? ClientID { get; set; }

        [MaxLength(100)]
        public string? ClientSecret { get; set; }

        public byte? Province_Id { get; set; }

        public short? IsExcludedRule27 { get; set; }

        public int? IsItCompany { get; set; }

        [MaxLength(200)]
        public string? DiClientId { get; set; }

        [MaxLength(200)]
        public string? DiClientSecret { get; set; }

        [MaxLength(200)]
        public string? DiApplicationId { get; set; }

        public bool? IsRegForIncomeTax { get; set; }

        public bool? IsRegForSalesTax { get; set; }

        public int? TotalPosBranches { get; set; }

        public int? TotalInvoiceIssued { get; set; }

        [MaxLength(50)]
        public string? FormationCode { get; set; }

        [MaxLength(50)]
        public string? FormationCodeDescription { get; set; }

        public long? LicenseIntegratorId { get; set; }

        public bool? IsProceedToProduction { get; set; }

        public DateTime? ProceedToProductionDate { get; set; }

        public short? LiAcceptStatus { get; set; }

        public DateTime? LiAcceptStatusDate { get; set; }

        public short? IsTier1Retailer { get; set; }
    }
}

