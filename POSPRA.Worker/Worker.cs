using Microsoft.Extensions.Options;
using POSPRA.Application.Services.CloudSyncService;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.DTOs;
using POSPRA.DTOs.CommanDtos;

namespace POSPRA.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _settings;

        public Worker(
            IServiceScopeFactory scopeFactory,
            IOptions<AppSettings> options)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var id = Guid.NewGuid().ToString();

            // Startup log
            using (var logScope = _scopeFactory.CreateScope())
            {
                var cloudSync = logScope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();
                await cloudSync.LogStartup(id);
            }

            try
            {
                while (!token.IsCancellationRequested)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var configSvc = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
                        var cloudSync = scope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();

                        // ✅ Check if Cloud Sync is enabled
                        if (await configSvc.IsCloudSyncEnabledAsync(new GetByPosIdDto { PosId = 110050 }))
                        {
                            await cloudSync.SyncInvoicesAsync(token, id);
                        }
                    }

                    // delay before next iteration
                    await Task.Delay(_settings.WorkerDelayTime, token);
                }
            }
            finally
            {
                // Shutdown log
                using (var logScope = _scopeFactory.CreateScope())
                {
                    var cloudSync = logScope.ServiceProvider.GetRequiredService<ISendInvoiceToCloudService>();
                    await cloudSync.LogShutdown(id);
                }
            }
        }
    }
}
