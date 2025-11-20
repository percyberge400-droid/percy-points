using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using Pos.Application.Services.CloudSyncService.CloudSyncLogService;
using Pos.Infrastructure.Persistence.Repositories.ProductCatalogue;

namespace Pos.API.Controllers
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
        public async Task<IActionResult> SyncInvoicesAsync(CancellationToken token,string environment, string workerId)
        {            
            await _sendInvoiceToCloudService.SyncInvoicesAsync(token, environment, workerId);
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
