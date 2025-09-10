using POSPRA.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace POSPRA.Domain.Entities
{
    public class Invoice
    {
        private int _BPOSID;

        [Range(100000, 999999, ErrorMessage = "POS ID should be equal to 6 digits.")]
        [Required(ErrorMessage = "POSID is required")]
        [Display(Name = "POSID")]
        public int BPOSID
        {
            get
            {
                return _BPOSID;
            }
            set
            {
                _BPOSID = value.ToString().Length != 6 ? GlobalVariables.POS_ID : value;
            }
        }

        // public int BPOSID { get; set; }
        public short InvoiceType { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? NTN_CNIC { get; set; }
        public string? BuyerSellerName { get; set; }
        public string? DestinationAddress { get; set; }
        public int SaleType { get; set; }
        public decimal? TotalSalesTaxApplicable { get; set; }
        public decimal TotalRetailPrice { get; set; }
        public decimal? TotalSTWithheldAtSource { get; set; }
        public decimal? TotalExtraTax { get; set; }
        public decimal? TotalFEDPayable { get; set; }
        public decimal? TotalWithheldIncomeTax { get; set; }
        public decimal? TotalCVT { get; set; }
        public string? Distributor_NTN_CNIC { get; set; }
        public string? DistributorName { get; set; }

        public List<InvoiceItemDetail>? InvoiceItemDetails { get; set; }
    }
    public class InvoiceItemDetail
    {
        public string HSCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
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
        public string WHIT_Section_1 { get; set; }
        public string WHIT_Section_2 { get; set; }
        public decimal TotalValues { get; set; }
    }
}
