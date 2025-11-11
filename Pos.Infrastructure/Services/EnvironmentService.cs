using Pos.Application.Interfaces;

namespace Pos.Infrastructure.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly string _filePath;
        private EnvironmentType _current;


        public EnvironmentService()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "environment.config");
            _current = LoadFromFile();
        }


        private EnvironmentType LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath)) return EnvironmentType.Production;
                var text = File.ReadAllText(_filePath);
                if (Enum.TryParse<EnvironmentType>(text, out var t)) return t;
                return EnvironmentType.Production;
            }
            catch { return EnvironmentType.Production; }
        }


        public EnvironmentType GetCurrentEnvironment() => _current;


        public void SetCurrentEnvironment(EnvironmentType type)
        {
            _current = type;
            try { File.WriteAllText(_filePath, type.ToString()); } catch { }
        }
    }
}
