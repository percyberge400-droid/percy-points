namespace POSPRA.DTOs.InvoiceDTOs
{
    public class InvoiceDto
    {
        public int BPOSID { get; set; }
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

        public List<InvoiceItemDetailDto>? InvoiceItemDetails { get; set; }
    }

    public class InvoiceItemDetailDto
    {
        public string? HSCode { get; set; }
        public string? ProductCode { get; set; }
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
        public string? WHIT_Section_1 { get; set; }
        public string? WHIT_Section_2 { get; set; }
        public decimal TotalValues { get; set; }
    }
}
