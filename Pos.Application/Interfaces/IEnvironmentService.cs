namespace Pos.Application.Interfaces
{
    public enum EnvironmentType
    {
        Sandbox,
        Production
    }


    public interface IEnvironmentService
    {
        EnvironmentType GetCurrentEnvironment();
        void SetCurrentEnvironment(EnvironmentType type);
    }
}