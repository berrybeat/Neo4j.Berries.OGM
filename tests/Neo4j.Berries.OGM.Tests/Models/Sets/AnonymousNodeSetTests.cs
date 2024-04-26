using System.Text;
using FluentAssertions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Sets;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Models.Sets;

public class AnonymousNodeSetTests
{
    public AnonymousNodeSetTests()
    {
        CypherBuilder = new StringBuilder();
        AnonymousNodeSet = new NodeSet(
            "Movie",
            new(),
            0,
            null,
            CypherBuilder
        );
    }

    public StringBuilder CypherBuilder { get; }
    public NodeSet AnonymousNodeSet { get; }


    [Fact]
    public void Should_Add_Unwind_Only_If_It_Is_First_Add()
    {
        var movies = new List<Dictionary<string, object>> {
            new() { {"Name", "The Matrix"}, {"ReleaseDate", new DateTime(1999, 3, 31)} },
            new() { {"Name", "The Matrix Reloaded"}, {"ReleaseDate", new DateTime(2003, 5, 15)} }
        };
        AnonymousNodeSet.AddRange(movies);
        CypherBuilder.ToString().Trim().Should().Be("UNWIND $anonymousList_0 as uw_movie_0");
    }
    [Fact]
    public void Should_Reset_NodeSet()
    {
        var movies = new List<Dictionary<string, object>> {
            new() { {"Name", "The Matrix"}, {"ReleaseDate", new DateTime(1999, 3, 31)} },
            new() { {"Name", "The Matrix Reloaded"}, {"ReleaseDate", new DateTime(2003, 5, 15)} }
        };
        AnonymousNodeSet.AddRange(movies);

        AnonymousNodeSet.Reset();

        AnonymousNodeSet.Nodes.Should().BeEmpty();
        AnonymousNodeSet.CreateCommand.Properties.Should().BeEmpty();
        AnonymousNodeSet.CreateCommand.SingleRelations.Should().BeEmpty();
        AnonymousNodeSet.CreateCommand.MultipleRelations.Should().BeEmpty();
    }
}