using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncLogService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudSyncController : ControllerBase
    {
        private readonly ISendInvoiceToCloudService _sendInvoiceToCloudService;
        private readonly ISendLogToCloudService _sendLogToCloudService;
        public CloudSyncController(ISendInvoiceToCloudService sendInvoiceToCloudService, ISendLogToCloudService sendLogToCloudService)
        {
            _sendInvoiceToCloudService = sendInvoiceToCloudService;
            _sendLogToCloudService = sendLogToCloudService;
        }

        [HttpPost("sync-invoices-async")]
        public async Task<IActionResult> SyncInvoicesAsync(CancellationToken token, string workerId)
        {
            await _sendInvoiceToCloudService.SyncInvoicesAsync(token, workerId);
            return Ok();
        }

        //[HttpGet("is-log-sync-enable")]
        //public async Task<IActionResult> IsLogSyncEnable()
        //{
        //    await _sendLogToCloudService.IsLogSyncEnable();
        //    return Ok();
        //}
    }
}
