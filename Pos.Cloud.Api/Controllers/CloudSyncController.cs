using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService;

namespace Pos.Cloud.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudSyncController : ControllerBase
    {
        private readonly ISendInvoiceToCloudService _sendInvoiceToCloudService;
        public CloudSyncController(ISendInvoiceToCloudService sendInvoiceToCloudService)
        {
            _sendInvoiceToCloudService = sendInvoiceToCloudService;
        }

        [HttpPost("sync-invoices-async")]
        public async Task<IActionResult> SyncInvoicesAsync(CancellationToken token, string workerId, string env)
        {
            await _sendInvoiceToCloudService.SyncInvoicesAsync(token, workerId, env);

            return Ok();
        }
    }
}