using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Services.UserService;
using POSPRA.Application.Utility;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;

var builder = WebApplication.CreateBuilder(args);

// ✅ Always use the same DB file (AppData\POSPRA\pospra.db)
var dbPath = SqliteDbContext.GetDbPath();

// ✅ SQLite
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ✅ SQL Server
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")),
    ServiceLifetime.Scoped);

// Register AutoMapper, repositories, services...
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserProfile>();
});

// ✅ SQLite
builder.Services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();

// ✅ SQL Server
builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();
// ✅ Add this line
builder.Services.AddScoped<ILogRepository, LogRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFiscalService, FiscalService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IPosService, PosService>();
builder.Services.AddScoped(typeof(SqlServerRepository<>));

builder.Services.AddScoped<InvoiceValidatorService>();
builder.Services.AddHttpClient<IHttpService, HttpService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestHeaderService, RequestHeaderService>();

// SendModelToServer depends on IHttpService
builder.Services.AddScoped<SendModelToServer>();

// Bind AppSettings section
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Add controllers
builder.Services.AddControllers();

// ✅ Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // ✅ Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI();
    // Redirect root URL ("/") to Swagger without creating an API endpoint
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

app.Run();