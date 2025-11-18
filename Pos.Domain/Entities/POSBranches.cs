using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Domain.Entities
{
    [Table("POSBranches")]
    public class POSBranches
    {
        [Key]
        public long POSBranchID { get; set; }

        [MaxLength(200)]
        public string? BranchName { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? EntryDate { get; set; }

        public long? POSMASTERID { get; set; }

        public short? Franchise { get; set; }

        public DateTime? StoreOpenning { get; set; }

        public DateTime? StoreClose { get; set; }

        public int? WeeklyOffDay { get; set; }

        public long? ContactPerson { get; set; }

        public long? Computer_No { get; set; }

        [MaxLength(150)]
        public string? BranchAddress { get; set; }

        public int? CityID { get; set; }

        [MaxLength(20)]
        public string? Latitude { get; set; }

        [MaxLength(20)]
        public string? Longitude { get; set; }

        [MaxLength(15)]
        public string? Version { get; set; }

        public int? Sector { get; set; }

        public long? POSRegistrationNumber { get; set; }

        public int? SubSectorId { get; set; }

        public DateTime? PeakStart { get; set; }

        public DateTime? PeakClose { get; set; }

        [MaxLength(20)]
        public string? LatitudeByFBR { get; set; }

        [MaxLength(20)]
        public string? LongitudeByFBR { get; set; }

        [MaxLength(350)]
        public string? BranchAddressByFBR { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
