using Pos.Application.AutoMapperProfile;
using Pos.Application.DTOs;
using Pos.Infrastructure;
using Pos.SecurityEncryption;
using System.Reflection;

var apiBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = apiBasePath
});

// --------------------------
// Load original JSON settings
// --------------------------
var originalConfig = new ConfigurationBuilder()
    .SetBasePath(apiBasePath)
    .AddJsonFile("cloud.appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// --------------------------
// Decrypt settings and merge
// --------------------------
var decryptedSettings = EncryptedSettingsHelper.DecryptSettingsFile("cloud.appsettings.json");

var flattened = decryptedSettings.ToDictionary(
    x => $"AppSettings:{x.Key}",
    x => x.Value
);

builder.Configuration
    .AddConfiguration(originalConfig)
    .AddInMemoryCollection(flattened)
    .AddEnvironmentVariables();

// --------------------------
// Add services
// --------------------------
builder.Services.AddControllers();

// Bind AppSettings POCO
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS Cloud API", Version = "2.0" });
//});

// --------------------------
// Configure Kestrel
// --------------------------
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

// Build App
var app = builder.Build();

// --------------------------
// Middleware
// --------------------------
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
//    c.RoutePrefix = string.Empty;
//});

app.UseHttpsRedirection();
app.UseAuthorization();


// ----------------------------------------------------
// 🔍 DEBUG BLOCK — Detect missing assemblies / bad types
// ----------------------------------------------------
try
{
    var allTypes = Assembly.GetExecutingAssembly().GetTypes();
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine("========== ReflectionTypeLoadException Detected ==========");
    Console.WriteLine("Types that failed to load:");

    foreach (var t in ex.Types)
    {
        Console.WriteLine(t != null ? $"Loaded: {t.FullName}" : "Loaded: null");
    }

    Console.WriteLine("\nLoader Exceptions:");
    foreach (var loaderEx in ex.LoaderExceptions)
    {
        Console.WriteLine(loaderEx.ToString());
    }

    Console.WriteLine("==========================================================");
    throw;
}


// ----------------------------------------------------
// Map Controllers (Now Wrapped to Catch Failing Types)
// ----------------------------------------------------
try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine("========== Controller Load Error ==========");

    foreach (var t in ex.Types)
    {
        Console.WriteLine(t != null ? $"Loaded: {t.FullName}" : "Loaded: null");
    }

    Console.WriteLine("\nLoader Exceptions:");
    foreach (var loaderEx in ex.LoaderExceptions)
    {
        Console.WriteLine(loaderEx.ToString());
    }

    Console.WriteLine("=============================================");
    throw;
}


// Run application
app.Run();
