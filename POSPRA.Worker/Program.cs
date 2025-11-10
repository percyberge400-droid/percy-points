using Pos.Application.DTOs;
using Pos.Infrastructure; // <-- for AddInfrastructure()
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Worker;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // ✅ Allow running as Windows Service
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        // ✅ Load worker-specific configuration
        config.Sources.Clear();

        config.AddJsonFile("appsettings.worker.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.worker.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        //----------------------------------------------------
        // 🔧 Load AppSettings and inject SQLite connection dynamically
        //----------------------------------------------------
        var configuration = context.Configuration;
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

        // Build SQLite connection path dynamically (if needed)
        var dbPath = appSettings?.DefaultDBFilePath;
        if (string.IsNullOrWhiteSpace(dbPath))
            dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");

        // Inject SQLite connection into configuration (for AddInfrastructure)
        var configurationWithSqlite = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SqliteConnection"] = $"Data Source={dbPath}"
            })
            .Build();

        //----------------------------------------------------
        // ✅ Centralized Infrastructure Registration
        //----------------------------------------------------
        services.AddInfrastructure(configurationWithSqlite);

        //----------------------------------------------------
        // ✅ AutoMapper
        //----------------------------------------------------
        services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

        //----------------------------------------------------
        // ✅ Worker-specific Hosted Services
        //----------------------------------------------------
        services.AddHostedService<Worker>();
        services.AddHostedService<SqliteBackupService>();
    });

var host = builder.Build();


// ✅ 1. Start API self-host (optional, runs alongside worker)
_ = Task.Run(async () =>
{
    try
    {
        var apiHost = Pos.Api.Program.BuildApiHost(args);
        await apiHost.StartAsync();
        Console.WriteLine("API self-hosted successfully at http://localhost:5010");
    }
    catch (Exception ex)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "api-start-error.log");
        File.AppendAllText(logPath, $"[{DateTime.Now}] {ex}\n");
    }
});


// ✅ 2. Log SQLite database path (for diagnostics)
//using (var scope = host.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
//    var dbPath = db.Database.GetDbConnection().DataSource;

//    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "service-log.txt"),
//        $"[{DateTime.Now}] Using existing SQLite DB: {dbPath}{Environment.NewLine}");
//}


// ✅ 3. Run Worker Service
await host.RunAsync();
