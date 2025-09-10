using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.UserService;
using POSPRA.Application.Utility;
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
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();