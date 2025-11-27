using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.ClientService;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;

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
        public async Task<IActionResult> Create([FromBody] List<FileRecordDto> dto, string environment) =>
            Ok(await _liveService.DecryptAndSaveInvoicesAsync(dto, environment));

        [HttpPost("export-csv")]
        public async Task<ActionResult<ApiResponse<string>>> GetInvoicesCsv(InvoiceFilterDto dto, string environment)
        {
            var response = await _liveService.GetInvoicesCsvAsync(dto, environment);
            return Ok(response);
        }

        [HttpPost("authenticate-by-mac")]
        public async Task<ActionResult<ApiResponse<string>>> AuthenticateByMacAsync(ClientValidationDto dto) =>
            Ok(await _clientService.GetByMacAsync(dto));

        [HttpPost("update-configuration-flag")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateConfigurationFlag(bool isConfiguration, string environment) =>
            Ok(await _clientService.UpdateConfigurationFlag(isConfiguration, null, environment));

        [HttpPost("create-cloud-log")]
        public async Task<ActionResult<ApiResponse<string>>> CreateCloudLog(List<SyncLogDto> logDtos, string environment) =>
            Ok(await _cloudLogService.CreateCloudLog(logDtos, environment));

        [HttpGet("get-isservice-enable")]
        public async Task<ActionResult<ApiResponse<bool>>> GetIsServiceEnable(long posId, string env) =>
            Ok(await _clientService.IsServiceEnabled(posId, env));

        [HttpGet("get-islog-enable")]
        public async Task<ActionResult<ApiResponse<bool>>> GetIsLogEnable(long posId, string env) =>
            Ok(await _clientService.IsLogEnabled(posId, env));

        [HttpGet("disbale-log-bit")]
        public async Task<ActionResult<ApiResponse<string>>> DisablePosCLientLogBit(long posId, string env) =>
            Ok(await _clientService.DisablePosCLientLogBit(env, posId));
    }
}