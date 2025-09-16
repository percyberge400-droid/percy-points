using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Services.UserService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;

var builder = WebApplication.CreateBuilder(args);

//----------------------------------------------------
// 🔧 Database configuration
//----------------------------------------------------

// ✅ Use single SQLite database file under AppData\POSPRA\pospra.db
var dbPath = SqliteDbContext.GetDbPath();

builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ✅ SQL Server for production data
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")),
    ServiceLifetime.Scoped);

//----------------------------------------------------
// 🔧 Dependency Injection
//----------------------------------------------------

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<InvoiceProfile>();
});

// Unit of Work
builder.Services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

// Generic / custom repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(SqlServerRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFiscalService, FiscalService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IPosService, PosService>();
builder.Services.AddScoped<InvoiceValidatorService>();
builder.Services.AddScoped<IRequestHeaderService, RequestHeaderService>();
builder.Services.AddScoped<SendModelToServer>();

// Http client
builder.Services.AddHttpClient<IHttpService, HttpService>();

// Access to HttpContext (needed by RequestHeaderService, etc.)
builder.Services.AddHttpContextAccessor();

// Strongly typed AppSettings
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

//----------------------------------------------------
// 🔧 Web / API configuration
//----------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks(); // optional but recommended

//----------------------------------------------------
// 🔧 Build and configure middleware
//----------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Redirect root URL to Swagger UI
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health"); // quick health check endpoint

app.Run();
