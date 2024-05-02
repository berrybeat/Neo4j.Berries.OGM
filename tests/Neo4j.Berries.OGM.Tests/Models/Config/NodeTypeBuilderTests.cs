using FluentAssertions;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Models.Config;

public class NodeTypeBuilderTests : TestBase
{

    [Fact]
    public void Should_Set_Identifiers()
    {
        var sut = new NodeTypeBuilder<Movie>();
        sut.HasIdentifier(x => x.Id);
        sut.HasIdentifier(x => x.Name);

        Neo4jSingletonContext.Configs["Movie"].Identifiers.Should().Contain("Id");
        Neo4jSingletonContext.Configs["Movie"].Identifiers.Should().Contain("Name");
    }
}