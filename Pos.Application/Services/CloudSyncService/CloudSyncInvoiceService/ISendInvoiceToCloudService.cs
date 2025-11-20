namespace Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService
{
    public interface ISendInvoiceToCloudService
    {
        Task SyncInvoicesAsync(CancellationToken token,string environment, string workerId);
    }
}