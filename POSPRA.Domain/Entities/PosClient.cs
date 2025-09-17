namespace POSPRA.Domain.Entities
{
    public class PosClient
    {
        public long POSRegistrationNumber { get; set; }
        public string? POSIdentificatioNumber { get; set; }
        public string? NTN { get; set; }
        public string? BusinessName { get; set; }
        public string? BranchName { get; set; }
        public string? BranchAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? StoreStatus { get; set; }
        public string ImagePath { get; set; }
        public int? CityID { get; set; }
        public string? PublicKey { get; set; }
        public string? PrivateKey { get; set; }
        public string? E_Key { get; set; }
        public string? MAC_Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? City { get; set; }
        public string? AppID { get; set; }
        public string? AppKey { get; set; }
        public string? IPAddress { get; set; }
        public string? PosMode { get; set; }
        public string? Version { get; set; }
        public string? Computer_no { get; set; }
        public int? Tax_Office_ID { get; set; }
        public string? Sector { get; set; }
        public string? POSType { get; set; }
        public int? POSBranchID { get; set; }
        public string? MacAddressInput { get; set; }
        public bool? IsConnected { get; set; }
        public DateTime? HeartbeatUpdatedOn { get; set; }
        public string? Token { get; set; }
        public string? PASSWORD { get; set; }
        public int? Province_Id { get; set; }
        public long? FileSize { get; set; }
    }
}
