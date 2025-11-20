using System.Configuration;
using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Services
{
    public class AppConfigEnvironmentService : IEnvironmentService
    {
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