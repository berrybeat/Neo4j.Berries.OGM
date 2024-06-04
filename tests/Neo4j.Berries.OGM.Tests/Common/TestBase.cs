using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Tests.Common;

[Collection("Serial")]
public abstract class TestBase
{
    public Neo4jOptions Neo4jOptions { get; set; }
    public ApplicationGraphContext TestGraphContext { get; }
    public TestBase(bool withSeed = false, Func<string, string> propertyCaseConverter = null)
    {
        var configurationBuilder = new OGMConfigurationBuilder(null)
            .ConfigureFromAssemblies(GetType().Assembly);
        configurationBuilder.PropertyCaseConverter = propertyCaseConverter;
        _ = new Neo4jSingletonContext(configurationBuilder);
        Neo4jSingletonContext.EnforceIdentifiers = false;
        if(propertyCaseConverter == null)
            Neo4jSingletonContext.PropertyCaseConverter = (x) => x;
        Neo4jOptions = new Neo4jOptions(ConfigurationsFactory.Config);
        TestGraphContext = new ApplicationGraphContext(Neo4jOptions);
        Neo4jSessionFactory.OpenSession(async session =>
        {
            await session.RunAsync("match(a) detach delete a");
        });
        if (withSeed)
            new Seed(TestGraphContext).ExecuteFullAsync().Wait();
    }

    public void OpenSession(Action<ISession> callback)
    {
        var session = Neo4jOptions.Driver.Session(opt =>
        {
            if (!string.IsNullOrEmpty(Neo4jOptions.Database))
                opt.WithDatabase(Neo4jOptions.Database);
        });
        callback(session);
    }
    public T OpenSession<T>(Func<ISession, T> callback)
    {
        var asyncSession = Neo4jOptions.Driver.Session(opt =>
        {
            if (!string.IsNullOrEmpty(Neo4jOptions.Database))
                opt.WithDatabase(Neo4jOptions.Database);
        });
        return callback(asyncSession);
    }
}