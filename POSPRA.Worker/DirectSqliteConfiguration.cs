namespace POSPRA.Worker
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;

    public class DirectSqliteConfiguration : IConfiguration
    {
        private readonly IConfigurationRoot _configuration;

        public DirectSqliteConfiguration(string dbPath)
        {
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SqliteConnection"] = $"Data Source={dbPath}"
                })
                .Build();
        }

        public string this[string key]
        {
            get => _configuration[key];
            set => _configuration[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => _configuration.GetChildren();
        public IChangeToken GetReloadToken() => _configuration.GetReloadToken();
        public IConfigurationSection GetSection(string key) => _configuration.GetSection(key);
    }
}
