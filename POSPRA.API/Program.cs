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

// Register DbContext
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite("Data Source=pospra.db"));

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

// ✅ Ensure database is created at API startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
