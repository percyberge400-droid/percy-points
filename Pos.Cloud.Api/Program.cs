using Microsoft.OpenApi.Models;
using Pos.Application.AutoMapperProfile;
using Pos.Application.DTOs;
using Pos.Cloud.Api.Middleware;
using Pos.Infrastructure;
using Pos.SecurityEncryption;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// Load original JSON settings
// --------------------------
var apiBasePath = builder.Environment.ContentRootPath;

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "POS Cloud API",
        Version = "2.0",
        Description = "POS Cloud API Swagger Documentation"
    });
});

// --------------------------
// Build App
// --------------------------
var app = builder.Build();

// --------------------------
// Middleware
// --------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
    c.RoutePrefix = string.Empty; // Swagger UI opens at root URL
});

app.UseHttpsRedirection();
app.UseMiddleware<ApiAuthenticationMiddleware>();
app.UseAuthorization();

// --------------------------
// Map Controllers
// --------------------------
app.MapControllers();

// --------------------------
// Run Application
// --------------------------
app.Run();
