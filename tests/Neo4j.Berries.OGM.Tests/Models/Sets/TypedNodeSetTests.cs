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
            null,
            CypherBuilder
        );
    }

    public StringBuilder CypherBuilder { get; }
    public NodeSet<Movie> MovieNodeSet { get; }


    [Fact]
    public void Should_Reset_NodeSet()
    {
        var movies = new List<Movie> {
            new () { Name = "The Matrix", ReleaseDate = new DateTime(1999, 3, 31) },
            new () { Name = "The Matrix Reloaded", ReleaseDate = new DateTime(2003, 5, 15) }
        };
        MovieNodeSet.AddRange(movies);

        MovieNodeSet.Reset();

        MovieNodeSet.NewNodes.Should().BeEmpty();
        MovieNodeSet.NewNode.Should().BeNull();
        MovieNodeSet.MergeNodes.Should().BeEmpty();
        MovieNodeSet.MergeNode.Should().BeNull();
    }
}