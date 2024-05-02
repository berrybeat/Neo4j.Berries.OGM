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
    public void Should_Reset_NodeSet()
    {
        var movies = new List<Dictionary<string, object>> {
            new() { {"Name", "The Matrix"}, {"ReleaseDate", new DateTime(1999, 3, 31)} },
            new() { {"Name", "The Matrix Reloaded"}, {"ReleaseDate", new DateTime(2003, 5, 15)} }
        };
        AnonymousNodeSet.AddRange(movies);

        AnonymousNodeSet.Reset();

        AnonymousNodeSet.NewNodes.Should().BeEmpty();
        AnonymousNodeSet.NewNode.Should().BeNull();
        AnonymousNodeSet.MergeNodes.Should().BeEmpty();
        AnonymousNodeSet.MergeNode.Should().BeNull();
    }
}