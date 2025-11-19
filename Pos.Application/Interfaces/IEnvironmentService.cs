using Pos.Domain.ValueObjects;

namespace Pos.Application.Interfaces
{

    public interface IEnvironmentService
    {
        // Synchronous process-wide environment
        Task SetCurrentEnvironment(EnvironmentType type);

        // Async per-user environment (for worker service / WinForms)
        Task<EnvironmentType> GetCurrentEnvironmentAsync();
    }
}