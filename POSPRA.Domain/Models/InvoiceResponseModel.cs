namespace POSPRA.Domain.Models
{
    public class InvoiceResponseModel
    {
        public InvoiceResponseModel(string invoiceNumber, string code, string response, string errors)
        {
            InvoiceNumber = invoiceNumber;
            Code = code;
            Response = response;
            Errors = errors;
        }

        public string InvoiceNumber { get; set; }
        public string Code { get; set; }
        public string Response { get; set; }
        public string Errors { get; set; }

    }
}
