using System.Reflection;
using Microsoft.OpenApi.Models;
using Pos.Application.DTOs;
using Pos.Infrastructure;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.SecurityEncryption;

namespace Pos.Local.Api
{
    public static class Program
    {
        public static WebApplication BuildApiHost(string[] args)
        {
            // --------------------------
            // Determine API folder
            // --------------------------
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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // --------------------------
            // Decrypt settings and merge
            // --------------------------
            var decryptedSettings = EncryptedSettingsHelper.DecryptSettingsFile("appsettings.json");

            var flattened = decryptedSettings.ToDictionary(
    x => $"AppSettings:{x.Key}",
    x => x.Value
);


            builder.Configuration
                .AddConfiguration(originalConfig)       // original JSON
                .AddInMemoryCollection(flattened) // override encrypted keys
                .AddEnvironmentVariables();             // environment variables override everything

            // --------------------------
            // Add services
            // --------------------------
            builder.Services.AddControllers()
                .PartManager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(Program).Assembly));

            // Bind AppSettings from configuration
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS API", Version = "2.0" });
            });

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

            // --------------------------
            // Configure Kestrel from appsettings.json
            // --------------------------
            builder.WebHost.ConfigureKestrel((context, options) =>
            {
                options.Configure(context.Configuration.GetSection("Kestrel"));
            });

            var app = builder.Build();

            // --------------------------
            // Middleware
            // --------------------------
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
                c.RoutePrefix = string.Empty; // swagger at root URL
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }

        // --------------------------
        // Main for standalone API run
        // --------------------------
        public static void Main(string[] args)
        {
            var app = BuildApiHost(args);
            app.Run();
        }
    }
}
