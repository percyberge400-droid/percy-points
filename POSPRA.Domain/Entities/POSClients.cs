namespace POSPRA.Domain.Entities
{
    public class POSClients
    {
        public long POSRegistrationNumber { get; set; }   // bigint (PK, Identity)
        public string? POSIdentificatioNumber { get; set; } // varchar(50)
        public string? NTN { get; set; }                   // varchar(13)
        public string? BusinessName { get; set; }          // varchar(150)
        public string? BranchName { get; set; }            // varchar(50)
        public string? BranchAddress { get; set; }         // varchar(150)
        public string? Latitude { get; set; }              // varchar(15)
        public string? Longitude { get; set; }             // varchar(15)
        public string? StoreStatus { get; set; }           // varchar(50)
        public string? ImagePath { get; set; }             // varchar(200)
        public int? CityID { get; set; }                   // int
        public string? PublicKey { get; set; }             // nvarchar(max)
        public string? PrivateKey { get; set; }            // nvarchar(max)
        public string? E_Key { get; set; }                 // nvarchar(max)
        public string? MAC_Address { get; set; }           // nvarchar(50)
        public bool? IsActive { get; set; }                // bit
        public DateTime? RegistrationDate { get; set; }    // datetime
        public DateTime? DateCreated { get; set; }         // datetime
        public string? City { get; set; }                  // varchar(50)
        public string? AppID { get; set; }                 // varchar(100)
        public string? AppKey { get; set; }                // varchar(100)
        public string? IPAddress { get; set; }             // varchar(50)
        public int? PosMode { get; set; }                  // int
        public string? Version { get; set; }               // varchar(20)
        public long? Computer_no { get; set; }             // bigint
        public short Tax_Office_ID { get; set; }           // smallint (NOT NULL)
        public int? Sector { get; set; }                   // int
        public short? POSType { get; set; }                // smallint
        public long? POSBranchID { get; set; }             // bigint
        public string? MacAddressInput { get; set; }       // varchar(50)
        public bool? IsConnected { get; set; }             // bit
        public DateTime? HeartbeatUpdatedOn { get; set; }  // datetime
        public string? Token { get; set; }                 // varchar(50)
        public string? PASSWORD { get; set; }              // nvarchar(20)
        public byte? Province_Id { get; set; }             // tinyint
        public long? FileSize { get; set; }                // bigint
    }
}
