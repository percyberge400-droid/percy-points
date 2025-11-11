using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.ClientService;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDTOs;

namespace Pos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class LiveController(ILiveService liveService, IClientService clientService, ICloudLogService cloudLogService) : ControllerBase
    {
        private readonly ILiveService _liveService = liveService;
        private readonly IClientService _clientService = clientService;
        private readonly ICloudLogService _cloudLogService = cloudLogService;

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
            Ok(await _clientService.UpdateConfigurationFlag(isConfiguration, null));

        [HttpPost("create-cloud-log")]
        public async Task<ActionResult<ApiResponse<string>>> CreateCloudLog(List<SyncLogDto> logDtos) =>
            Ok(await _cloudLogService.CreateCloudLog(logDtos));

        [HttpGet("get-isservice-enable")]
        public async Task<ActionResult<ApiResponse<bool>>> CreateCloudLog(long posId) =>
            Ok(await _clientService.IsServiceEnabled(posId));

    }
}