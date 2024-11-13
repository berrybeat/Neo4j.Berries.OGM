using FluentAssertions;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Enums;
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

    [Fact]
    public void Should_Set_Relation_Property_On_HasSingle()
    {
        var sut = new NodeTypeBuilder<Movie>();
        sut.HasRelationWithSingle(x => x.Director, "DIRECTED", RelationDirection.In);

        Neo4jSingletonContext.Configs["Movie"].Relations["Director"].Property.Should().Be("Director");
        Neo4jSingletonContext.Configs["Movie"].Relations["Director"].IsCollection.Should().Be(false);
    }

    [Fact]
    public void Should_Set_Relation_Property_And_IsCollection_On_HasMultiple() {

        var sut = new NodeTypeBuilder<Movie>();
        sut.HasRelationWithMultiple(x => x.Actors, "ACTED_IN", RelationDirection.In);

        Neo4jSingletonContext.Configs["Movie"].Relations["Actors"].Property.Should().Be("Actors");
        Neo4jSingletonContext.Configs["Movie"].Relations["Actors"].IsCollection.Should().Be(true);
    }

    [Fact]
    public void Should_Remove_Property_From_ExcludedProperties_On_Include()
    {

        var sut = new NodeTypeBuilder<Movie>();
        sut.Include(x => x.Director);

        Neo4jSingletonContext.Configs["Movie"].ExcludedProperties.Should().NotContain("Director");
    }
}