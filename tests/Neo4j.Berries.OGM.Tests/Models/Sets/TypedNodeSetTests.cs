using System.Text;
using FluentAssertions;
using Neo4j.Berries.OGM.Models.Sets;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Models.Sets;

public class TypedNodeSetTests
{
    public TypedNodeSetTests()
    {
        CypherBuilder = new StringBuilder();
        MovieNodeSet = new NodeSet<Movie>(
            0,
            "Movies",
            new(),
            null,
            CypherBuilder
        );
    }

    public StringBuilder CypherBuilder { get; }
    public NodeSet<Movie> MovieNodeSet { get; }


    [Fact]
    public void Should_Add_Unwind_Only_If_It_Is_First_Add()
    {
        var movies = new List<Movie> {
            new Movie { Name = "The Matrix", ReleaseDate = new DateTime(1999, 3, 31) },
            new Movie { Name = "The Matrix Reloaded", ReleaseDate = new DateTime(2003, 5, 15) }
        };
        MovieNodeSet.AddRange(movies);
        CypherBuilder.ToString().Trim().Should().Be("UNWIND $movies as uw_movie_0");
    }

    [Fact]
    public void Should_Reset_NodeSet()
    {
        var movies = new List<Movie> {
            new () { Name = "The Matrix", ReleaseDate = new DateTime(1999, 3, 31) },
            new () { Name = "The Matrix Reloaded", ReleaseDate = new DateTime(2003, 5, 15) }
        };
        MovieNodeSet.AddRange(movies);

        MovieNodeSet.Reset();

        MovieNodeSet.Nodes.Should().BeEmpty();
        MovieNodeSet.CreateCommand.Properties.Should().BeEmpty();
        MovieNodeSet.CreateCommand.SingleRelations.Should().BeEmpty();
        MovieNodeSet.CreateCommand.MultipleRelations.Should().BeEmpty();
    }
}