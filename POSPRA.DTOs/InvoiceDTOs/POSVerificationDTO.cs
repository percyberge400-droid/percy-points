namespace POSPRA.DTOs.InvoiceDTOs
{
    public class POSVerificationDTO
    {
        public int POSID { get; set; }
        public int POSComputer_no { get; set; }
        public int POSList { get; set; }
        public int POSListComputer_no { get; set; }
        public bool POSStatus { get; set; }
    }
}
