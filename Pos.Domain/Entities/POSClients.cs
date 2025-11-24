using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Domain.Entities
{
    [Table("PosClients")]
    public class PosClients
    {
        [Key]
        public long POSRegistrationNumber { get; set; }
        public string? POSIdentificatioNumber { get; set; }
        public string? NTN { get; set; }
        public string? BusinessName { get; set; }
        public string? BranchName { get; set; }
        public string? BranchAddress { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? StoreStatus { get; set; }
        public string? ImagePath { get; set; }
        public int? CityID { get; set; }
        public string? PublicKey { get; set; }
        public string? PrivateKey { get; set; }
        public string? E_Key { get; set; }
        public string? MAC_Address { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? City { get; set; }
        public string? AppID { get; set; }
        public string? AppKey { get; set; }
        public string? IPAddress { get; set; }
        public int? PosMode { get; set; }
        public string? Version { get; set; }
        public long? Computer_no { get; set; }
        public short Tax_Office_ID { get; set; }
        public int? Sector { get; set; }
        public short? POSType { get; set; }
        public long? POSBranchID { get; set; }
        public string? MacAddressInput { get; set; }
        public bool? IsConnected { get; set; }
        public DateTime? HeartbeatUpdatedOn { get; set; }
        public string? Token { get; set; }
        public string? PASSWORD { get; set; }
        public byte? Province_Id { get; set; }
        public long? FileSize { get; set; }
        public string? ClientType { get; set; }
        public bool? IsLogSynced { get; set; }
        public bool? IsServiceEnabled { get; set; }
        [NotMapped]
        public string? PhoneNumber { get; set; }

    }
}
