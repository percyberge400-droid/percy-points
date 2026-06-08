namespace Pos.Application.Interfaces
{
    public interface IMigrationService
    {
        Task RunAsync(string connectionString);
    }
}
