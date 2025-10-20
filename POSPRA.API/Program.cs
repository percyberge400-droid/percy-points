using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.UserService;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.UserRepository;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite("Data Source=pospra.db"));

// Register AutoMapper, repositories, services...
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 🔹 Ensure database is created at API startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
