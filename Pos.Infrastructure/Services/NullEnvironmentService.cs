using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;
using System.Configuration;

namespace Pos.Infrastructure.Services
{
    public class NullEnvironmentService : IEnvironmentService
    {
        private readonly string _appConfigPath; // path to App.config (WinForms)

        public NullEnvironmentService(string appConfigPath = null)
        {
            if (string.IsNullOrWhiteSpace(appConfigPath))
                throw new ArgumentNullException(nameof(appConfigPath));

            _appConfigPath = appConfigPath; // optional
        }


        public Task<EnvironmentType> GetCurrentEnvironmentAsync()
        {
            try
            {
                string env = ConfigurationManager.AppSettings["Environment"];

                if (!string.IsNullOrWhiteSpace(env) &&
                    Enum.TryParse(env, true, out EnvironmentType environment))
                {
                    return Task.FromResult(environment);
                }

                return Task.FromResult(EnvironmentType.Production);
            }
            catch
            {
                return Task.FromResult(EnvironmentType.Production);
            }
        }


        public Task SetCurrentEnvironment(EnvironmentType type)
        {
            throw new NotImplementedException();
        }
    }
}
