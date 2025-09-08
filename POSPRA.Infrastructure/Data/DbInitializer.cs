using POSPRA.Infrastructure.Context;

namespace POSPRA.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize()
        {
            using var context = new SqliteDbContext();

            // Ensure database + tables exist
            context.Database.EnsureCreated();

            // Optional: Apply migrations if you are using them
            // context.Database.Migrate();
        }
    }
}
