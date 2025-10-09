namespace POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService
{
    public interface ISendInvoiceToCloudService
    {
        Task SyncInvoicesAsync(CancellationToken token, string workerId);
    }
}