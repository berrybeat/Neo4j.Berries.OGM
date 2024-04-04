using FluentAssertions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Tests.Models.Config;

public class NodeConfigurationBuilderTests
{
    [Fact]
    public void Should_Create_NodeConfiguration_With_IncludedProperties()
    {
        var sut = new NodeConfigurationBuilder();
        sut.IncludeProperties("Name", "Age");
        sut.NodeConfiguration.IncludedProperties.Should().HaveCount(2);
        sut.NodeConfiguration.IncludedProperties.Should().Contain("Name");
        sut.NodeConfiguration.IncludedProperties.Should().Contain("Age");
    }
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
    public void Should_Throw_Exception_When_A_Property_Added_To_Includes_And_Now_Being_Excluded()
    {
        var sut = new NodeConfigurationBuilder();
        sut.IncludeProperties("Name");
        Action act = () => sut.ExcludeProperties("Name");
        act.Should().Throw<InvalidOperationException>().WithMessage("Property 'Name' is already included.");
    }

    [Fact]
    public void Should_Throw_Exception_When_A_Property_Added_To_Excludes_And_Now_Being_Included()
    {
        var sut = new NodeConfigurationBuilder();
        sut.ExcludeProperties("Name");
        Action act = () => sut.IncludeProperties("Name");
        act.Should().Throw<InvalidOperationException>().WithMessage("Property 'Name' is already excluded.");
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
        sut.NodeConfiguration.Relations["Actor"].EndNodeLabel.Should().Be("Actor");
    }

    [Fact]
    public void Should_Throw_Exception_On_Including_A_Relation_With_IncludeProperties()
    {
        var sut = new NodeConfigurationBuilder();
        sut.HasRelation("Actor", "Actor", "ACTED_IN", RelationDirection.Out);
        Action act = () => sut.IncludeProperties("Actor");
        act.Should().Throw<InvalidOperationException>().WithMessage("Property 'Actor' is already excluded.");
    }
}