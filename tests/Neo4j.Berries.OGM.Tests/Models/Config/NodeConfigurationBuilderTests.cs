using FluentAssertions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Tests.Models.Config;

public class NodeConfigurationBuilderTests
{
    [Fact]
    public void Should_Create_NodeConfiguration_With_ExcludedProperties()
    {
        var sut = new NodeConfigurationBuilder();
        sut.ExcludeProperties("Name", "Age");
        sut.NodeConfiguration.ExcludedProperties.Should().HaveCount(2);
        sut.NodeConfiguration.ExcludedProperties.Should().Contain("Name");
        sut.NodeConfiguration.ExcludedProperties.Should().Contain("Age");
    }

    [Fact]
    public void Should_Remove_From_ExcludedProperties_When_A_Property_Added_To_Excludes_And_Now_Being_Included()
    {
        var sut = new NodeConfigurationBuilder();
        sut.ExcludeProperties("Name");
        sut.NodeConfiguration.ExcludedProperties.Should().Contain("Name");
        sut.IncludeProperties("Name");
        sut.NodeConfiguration.ExcludedProperties.Should().NotContain("Name");
    }

    [Fact]
    public void Should_Create_NodeConfiguration_With_Relation()
    {
        var sut = new NodeConfigurationBuilder();
        sut.HasRelation("Actor", "Actor", "ACTED_IN", RelationDirection.Out);
        sut.NodeConfiguration.Relations.Should().HaveCount(1);
        sut.NodeConfiguration.Relations.Should().ContainKey("Actor");
        sut.NodeConfiguration.Relations["Actor"].Direction.Should().Be(RelationDirection.Out);
        sut.NodeConfiguration.Relations["Actor"].Label.Should().Be("ACTED_IN");
        sut.NodeConfiguration.Relations["Actor"].EndNodeLabels.Should().Contain("Actor");
    }

    [Fact]
    public void Should_Remove_From_ExcludedProperties_On_Including_A_Relation_With_IncludeProperties()
    {
        var sut = new NodeConfigurationBuilder();
        sut.HasRelation("Actor", "Actor", "ACTED_IN", RelationDirection.Out);
        sut.IncludeProperties("Actor");
        sut.NodeConfiguration.ExcludedProperties.Should().NotContain("Actor");
    }

    [Fact]
    public void Should_Set_Identifier()
    {
        var sut = new NodeConfigurationBuilder();
        sut.HasIdentifier("Id");
        sut.NodeConfiguration.Identifiers.Should().Contain("Id");
    }
}