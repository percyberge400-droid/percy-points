using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.ClientService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class LiveController(ILiveService liveService, IClientService clientService, ILogService logService) : ControllerBase
    {
        private readonly ILiveService _liveService = liveService;
        private readonly IClientService _clientService = clientService;
        private readonly ILogService _logService = logService;

        [HttpPost("decrypt-save")]
        public async Task<IActionResult> Create([FromBody] List<FileRecordDto> dto) =>
            Ok(await _liveService.DecryptAndSaveInvoicesAsync(dto));

        [HttpPost("export-csv")]
        public async Task<ActionResult<ApiResponse<string>>> GetInvoicesCsv(InvoiceFilterDto dto)
        {
            var response = await _liveService.GetInvoicesCsvAsync(dto);
            return Ok(response);
        }

        [HttpPost("authenticate-by-mac")]
        public async Task<ActionResult<ApiResponse<string>>> AuthenticateByMacAsync(ClientValidationDto dto) =>
            Ok(await _clientService.GetByMacAsync(dto));

        [HttpPost("update-configuration-flag")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateConfigurationFlag(bool isConfiguration) =>
            Ok(await _clientService.UpdateConfigurationFlag(isConfiguration,null));

        [HttpPost("create-cloud-log")]
        public async Task<ActionResult<ApiResponse<string>>> CreateCloudLog(List<SyncLogDto> logDtos) =>
            Ok(await _logService.CreateCloudLog(logDtos));

    }
}