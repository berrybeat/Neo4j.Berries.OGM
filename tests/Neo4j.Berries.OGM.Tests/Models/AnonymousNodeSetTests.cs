using System.Text;
using FluentAssertions;
using Neo4j.Berries.OGM.Models.Sets;

namespace Neo4j.Berries.OGM.Tests.Models;

public class AnonymousNodeSetTests
{
    [Fact]
    public void Should_Add_Generate_Cypher_For_An_Anonymous_Type()
    {
        var sutCypherBuilder = new StringBuilder();
        var sut = new NodeSet("Movie", 0, null, sutCypherBuilder);
        sut.Add(new
        {
            Title = "The Matrix",
            Year = 1999
        });
        sutCypherBuilder.ToString().Trim().Should().Be("CREATE (a_0_0:Movie { Title: $cp_0_0_0, Year: $cp_0_0_1 })");
        sut.CreateCommands.Should().HaveCount(1);
        var createCommand = sut.CreateCommands.Single();
        createCommand.Parameters.Should().HaveCount(2);
        createCommand.Parameters["cp_0_0_0"].Should().Be("The Matrix");
        createCommand.Parameters["cp_0_0_1"].Should().Be(1999);
    }

    [Fact]
    public void Should_Generate_Cypher_For_AddRange()
    {
        var sutCypherBuilder = new StringBuilder();
        var sut = new NodeSet("Movie", 0, null, sutCypherBuilder);
        sut.AddRange([
            new
            {
                Title = "The Matrix",
                Year = 1999
            },
            new
            {
                Title = "The Matrix Reloaded",
                Year = 2003
            },
        ]);
        sutCypherBuilder.ToString().Trim().Should().Be("""
        CREATE (a_0_0:Movie { Title: $cp_0_0_0, Year: $cp_0_0_1 })
        CREATE (a_0_1:Movie { Title: $cp_0_1_0, Year: $cp_0_1_1 })
        """);
        sut.CreateCommands.Should().HaveCount(2);
        var createCommands = sut.CreateCommands;
        createCommands[0].Parameters.Should().HaveCount(2);
        createCommands[0].Parameters["cp_0_0_0"].Should().Be("The Matrix");
        createCommands[0].Parameters["cp_0_0_1"].Should().Be(1999);

        createCommands[1].Parameters.Should().HaveCount(2);
        createCommands[1].Parameters["cp_0_1_0"].Should().Be("The Matrix Reloaded");
        createCommands[1].Parameters["cp_0_1_1"].Should().Be(2003);
    }
}