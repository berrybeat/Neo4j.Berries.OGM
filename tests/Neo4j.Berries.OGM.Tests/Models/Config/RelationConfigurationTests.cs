using FluentAssertions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;

namespace Neo4j.Berries.OGM.Tests.Models.Config;

public class RelationConfigurationTests
{
    [Fact]
    public void Should_Add_Single_Relation_EndNodeLabel()
    {
        var sut = new RelationConfiguration<Person, Movie>("ACTED_IN", RelationDirection.Out);
        sut.EndNodeLabels.Should().Contain("Movie");
    }

    [Fact]
    public void Should_Add_Multiple_EndNodeLabel_On_Relation()
    {
        var sut = new RelationConfiguration<Person, IResource>("USES", RelationDirection.Out);
        sut.EndNodeLabels.Should().NotContain("IResource");
        sut.EndNodeLabels.Should().Contain("Car");
        sut.EndNodeLabels.Should().Contain("Room");
    }
}