namespace Pos.Application.DTOs.ClientDtos
{
    public class ClientValidationDto
    {
        public long PosId { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }
}