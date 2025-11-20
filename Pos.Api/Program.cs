using System.Reflection;
using Microsoft.OpenApi.Models;
using Pos.Application.DTOs;
using Pos.Infrastructure;
using POSPRA.Application.AutoMapperProfile;

namespace Pos.Api
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

            // Load API appsettings.json
            builder.Configuration
                .SetBasePath(apiBasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            // --------------------------
            // Add services
            // --------------------------
            builder.Services.AddControllers()
                .PartManager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(Program).Assembly));

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
            // Optional: verify SQLite DB
            // --------------------------
            var configuration = builder.Configuration;

            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()
                ?? throw new InvalidOperationException("AppSettings section missing.");

            var sqliteConnectionString = appSettings?.DefaultDBFilePath
                ?? throw new InvalidOperationException("SqliteConnection not found in AppSettings.");

            // --------------------------
            // Middleware
            // --------------------------
            //if (app.Environment.IsDevelopment())
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
                c.RoutePrefix = string.Empty; // swagger at root URL
            });

            var url = "http://localhost:8524"; // Change port if needed

            // Open in default browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
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
