using berrybeat.Neo4j.OGM.Contexts;
using berrybeat.Neo4j.OGM.Tests.Mocks;
using Neo4j.Driver;

namespace berrybeat.Neo4j.OGM.Tests.Common;

[Collection("Serial")]
public abstract class TestBase
{
    public Neo4jOptions Neo4jOptions { get; set; }
    public ApplicationGraphContext TestGraphContext { get; }
    public TestBase(bool withSeed = false)
    {
        _ = new Neo4jSingletonContext(GetType().Assembly);
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