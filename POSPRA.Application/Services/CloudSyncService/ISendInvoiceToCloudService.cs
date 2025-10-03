namespace POSPRA.Application.Services.CloudSyncService
{
    public interface ISendInvoiceToCloudService
    {
        Task SyncInvoicesAsync(CancellationToken token, string workerId);
        Task LogStartup(string workerId);
        Task LogShutdown(string workerId);
    }
}