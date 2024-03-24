using Microsoft.Extensions.Configuration;

namespace berrybeat.Neo4j.OGM.Tests.Common;

public class ConfigurationsFactory
{
    private static IConfiguration _configuration;
    public static IConfiguration Config
    {
        get
        {
            var appsettingsPath = "appsettings.json";
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettingsPath, true, false)
                .AddEnvironmentVariables()
                .Build();
            return _configuration;
        }
    }
}
