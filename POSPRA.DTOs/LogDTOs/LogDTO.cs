namespace POSPRA.DTOs.LogDtos
{
    public class LogDto
    {
        public long Id { get; set; }

        public string? Message { get; set; }

        public string? Type { get; set; }

        /// <summary>
        /// Timestamp from CreatedAtPk column in the database.
        /// </summary>
        public DateTime CreatedAtPk { get; set; }
    }
}
