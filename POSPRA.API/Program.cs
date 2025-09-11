using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.UserService;
using POSPRA.Application.Utility;
using POSPRA.Domain.ValueObjects;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.UserRepository;

var builder = WebApplication.CreateBuilder(args);

// ✅ Always use the same DB file (AppData\POSPRA\pospra.db)
var dbPath = SqliteDbContext.GetDbPath();

builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register AutoMapper, repositories, services...
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserProfile>();
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFiscalService, FiscalService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<InvoiceValidatorService>();
builder.Services.AddHttpClient<IHttpService, HttpService>();

// IHttpService needs HttpClient
builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.BaseAddress = new Uri(GlobalVariables.GATEWAY_URL);
});

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