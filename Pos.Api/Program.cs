using Microsoft.EntityFrameworkCore;
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
            var builder = WebApplication.CreateBuilder(args);

            // --------------------------
            // Add services
            // --------------------------
            builder.Services.AddControllers();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS API", Version = "2.0" });
            });

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

            var app = builder.Build();

            // --------------------------
            // Optional: verify SQLite DB
            // --------------------------
            // 1️⃣ Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 2️⃣ Bind AppSettings
            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

            // 3️⃣ Get SQLite connection string from AppSettings
            var sqliteConnectionString = appSettings?.DefaultDBFilePath
                ?? throw new InvalidOperationException("SqliteConnection not found in AppSettings.");


            // --------------------------
            // Middleware
            // --------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
                    c.RoutePrefix = string.Empty;
                });
            }

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
