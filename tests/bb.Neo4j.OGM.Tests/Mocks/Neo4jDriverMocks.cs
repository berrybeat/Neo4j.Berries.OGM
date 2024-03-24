using Moq;
using Neo4j.Driver;

namespace berrybeat.Neo4j.OGM.Tests.Mocks;

public class Neo4jDriverMocks
{
    public static IAsyncSession GetAsyncSession()
    {
        var mock = new Mock<IAsyncSession>();
        return mock.Object;
    }

    public static IDriver GetDriver() {
        var mock = new Mock<IDriver>();
        return mock.Object;
    }
}