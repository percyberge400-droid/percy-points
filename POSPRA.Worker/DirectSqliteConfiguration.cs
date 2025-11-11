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
            var directory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:DefaultDBFilePath"] = $"Data Source={dbPath}"
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
