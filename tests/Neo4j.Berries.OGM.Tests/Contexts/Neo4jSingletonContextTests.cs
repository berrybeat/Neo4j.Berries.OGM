using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using FluentAssertions;

namespace Neo4j.Berries.OGM.Tests.Contexts;

public class Neo4jSingletonContextTests : TestBase
{


    [Fact]
    public void Should_Have_Relative_Configurations_Adjusted()
    {
        Neo4jSingletonContext.Configs.Should().NotBeEmpty();
        Neo4jSingletonContext.Configs.Should().ContainKey(nameof(Movie));
        Neo4jSingletonContext.Configs.Should().ContainKey(nameof(Person));

        var movieNodeConfig = Neo4jSingletonContext.Configs[nameof(Movie)];
        movieNodeConfig.Relations.Should().HaveCount(4);
        movieNodeConfig.Relations[nameof(Movie.Actors)].Label.Should().Be("ACTED_IN");
        movieNodeConfig.Relations[nameof(Movie.Actors)].Direction.Should().Be(RelationDirection.In);
        movieNodeConfig.Relations[nameof(Movie.Actors)].EndNodeType.Should().Be(typeof(Person));
        movieNodeConfig.Relations[nameof(Movie.Director)].Label.Should().Be("DIRECTED");
        movieNodeConfig.Relations[nameof(Movie.Director)].Direction.Should().Be(RelationDirection.In);
        movieNodeConfig.Relations[nameof(Movie.Director)].EndNodeType.Should().Be(typeof(Person));

        var personNodeConfig = Neo4jSingletonContext.Configs[nameof(Person)];
        personNodeConfig.Relations[nameof(Person.MoviesAsActor)].Label.Should().Be("ACTED_IN");
        personNodeConfig.Relations[nameof(Person.MoviesAsActor)].Direction.Should().Be(RelationDirection.Out);
        personNodeConfig.Relations[nameof(Person.MoviesAsActor)].Label.Should().Be("ACTED_IN");
        personNodeConfig.Relations[nameof(Person.MoviesAsActor)].EndNodeType.Should().Be(typeof(Movie));

        personNodeConfig.Relations[nameof(Person.MoviesAsDirector)].Direction.Should().Be(RelationDirection.Out);
        personNodeConfig.Relations[nameof(Person.MoviesAsDirector)].EndNodeType.Should().Be(typeof(Movie));

        personNodeConfig.Relations[nameof(Person.Awards)].EndNodeType.Should().Be(typeof(Award));
        personNodeConfig.Relations[nameof(Person.Awards)].EndNodeMergeProperties.Should().Contain(nameof(Award.Name));
        personNodeConfig.Relations[nameof(Person.Awards)].EndNodeMergeProperties.Should().Contain(nameof(Award.Category));
    }
    [Fact]
    public void Should_Add_Relations_In_Properties_Map_Exclusion_List()
    {
        var movieNodeConfig = Neo4jSingletonContext.Configs[nameof(Movie)];
        movieNodeConfig.ExcludedProperties.Should().Contain(nameof(Movie.Director));
        movieNodeConfig.ExcludedProperties.Should().Contain(nameof(Movie.Actors));
    }
    [Fact]
    public void Should_Have_EndNode_Config_While_Merging_In_Relations()
    {
        var movieNodeConfig = Neo4jSingletonContext.Configs[nameof(Movie)];
        movieNodeConfig.Relations[nameof(Movie.Actors)].EndNodeMergeProperties.Should().NotBeEmpty();
        movieNodeConfig.Relations[nameof(Movie.Actors)].EndNodeMergeProperties.Should().OnlyContain(x => x == nameof(Person.Id));
    }
}