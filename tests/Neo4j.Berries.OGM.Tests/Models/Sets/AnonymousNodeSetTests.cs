using System.Text;
using FluentAssertions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Sets;

namespace Neo4j.Berries.OGM.Tests.Models.Sets;

public class AnonymousNodeSetTests
{
    // [Fact]
    // public void Should_Add_Generate_Cypher_For_An_Anonymous_Type()
    // {
    //     var sutCypherBuilder = new StringBuilder();
    //     var sut = new NodeSet("Movie", new NodeConfiguration(), 0, null, sutCypherBuilder);
    //     sut.Add(new Dictionary<string, object>
    //     {
    //         {"Title", "The Matrix"},
    //         {"Year", 1999}
    //     });
    //     sutCypherBuilder.ToString().Trim().Should().Be("CREATE (a_0_0:Movie { Title: $cp_0_0_0, Year: $cp_0_0_1 })");
    //     sut.CreateCommands.Should().HaveCount(1);
    //     var createCommand = sut.CreateCommands.Single();
    //     createCommand.Parameters.Should().HaveCount(2);
    //     createCommand.Parameters["cp_0_0_0"].Should().Be("The Matrix");
    //     createCommand.Parameters["cp_0_0_1"].Should().Be(1999);
    // }

    // [Fact]
    // public void Should_Generate_Cypher_For_AddRange()
    // {
    //     var sutCypherBuilder = new StringBuilder();
    //     var sut = new NodeSet("Movie", new NodeConfiguration(), 0, null, sutCypherBuilder);
    //     sut.AddRange([
    //         new Dictionary<string, object>
    //         {
    //             {"Title", "The Matrix"},
    //             {"Year", 1999}
    //         },
    //         new Dictionary<string, object>
    //         {
    //             {"Title", "The Matrix Reloaded"},
    //             {"Year", 2003}
    //         },
    //     ]);
    //     sutCypherBuilder.ToString().Trim().Should().Be("""
    //     CREATE (a_0_0:Movie { Title: $cp_0_0_0, Year: $cp_0_0_1 })
    //     CREATE (a_0_1:Movie { Title: $cp_0_1_0, Year: $cp_0_1_1 })
    //     """);
    //     sut.CreateCommands.Should().HaveCount(2);
    //     var createCommands = sut.CreateCommands;
    //     createCommands[0].Parameters.Should().HaveCount(2);
    //     createCommands[0].Parameters["cp_0_0_0"].Should().Be("The Matrix");
    //     createCommands[0].Parameters["cp_0_0_1"].Should().Be(1999);

    //     createCommands[1].Parameters.Should().HaveCount(2);
    //     createCommands[1].Parameters["cp_0_1_0"].Should().Be("The Matrix Reloaded");
    //     createCommands[1].Parameters["cp_0_1_1"].Should().Be(2003);
    // }

    // [Fact]
    // public void Should_Generate_Cypher_With_Merging_TargetNodes_And_Creating_Relations()
    // {
    //     var sutCypherBuilder = new StringBuilder();
    //     var sut = new NodeSet(
    //         label: "Movie",
    //         nodeConfiguration: new NodeConfigurationBuilder()
    //             .HasRelation("Director", "Person", "DIRECTED_BY", RelationDirection.In)
    //             .NodeConfiguration,
    //         nodeSetIndex: 0,
    //         databaseContext: null,
    //         cypherBuilder: sutCypherBuilder);
    //     sut.Add(new Dictionary<string, object>
    //     {
    //         {"Title", "The Matrix"},
    //         {"Year", 1999},
    //         {"Director", new Dictionary<string, object> {
    //             {"Name", "The Wachowskis"}
    //         }}
    //     });
    //     sutCypherBuilder.ToString().Trim().Should().Be("""
    //     CREATE (a_0_0:Movie { Title: $cp_0_0_0, Year: $cp_0_0_1 })
    //     MERGE (person0_1:Person { Name: $cp_0_0_2 })
    //     CREATE (a_0_0)<-[:DIRECTED_BY]-(person0_1)
    //     """);
    //     sut.CreateCommands.Should().HaveCount(1);
    //     var createCommand = sut.CreateCommands.Single(); ;
    //     createCommand.Parameters.Should().HaveCount(3);
    //     createCommand.Parameters["cp_0_0_0"].Should().Be("The Matrix");
    //     createCommand.Parameters["cp_0_0_1"].Should().Be(1999);
    //     createCommand.Parameters["cp_0_0_2"].Should().Be("The Wachowskis");
    // }

    // [Fact]
    // public void Should_Generate_Cypher_With_Merging_Multiple_Target_Nodes_And_Create_Relations()
    // {
    //     var sutCypherBuilder = new StringBuilder();
    //     var sut = new NodeSet(
    //         label: "Movie",
    //         nodeConfiguration: new NodeConfigurationBuilder()
    //             .HasRelation("Director", "Person", "DIRECTED_BY", RelationDirection.In)
    //             .HasRelation("Actors", "Person", "ACTED_IN", RelationDirection.In)
    //             .NodeConfiguration,
    //         nodeSetIndex: 0,
    //         databaseContext: null,
    //         cypherBuilder: sutCypherBuilder);
    //     sut.Add(new Dictionary<string, object>
    //     {
    //         {"Title", "The Matrix"},
    //         {"Year", 1999},
    //         {"Director", new Dictionary<string, object> {
    //             {"Name", "The Wachowskis"}
    //         }},
    //         {"Actors", new List<Dictionary<string, object>> {
    //             new Dictionary<string, object> {
    //                 {"Name", "Keanu Reeves"}
    //             },
    //             new Dictionary<string, object> {
    //                 {"Name", "Laurence Fishburne"}
    //             }
    //         }}
    //     });

    //     sutCypherBuilder.ToString().Trim().Should().Be("""
    //     CREATE (a_0_0:Movie { Title: $cp_0_0_0, Year: $cp_0_0_1 })
    //     MERGE (person0_1:Person { Name: $cp_0_0_2 })
    //     CREATE (a_0_0)<-[:DIRECTED_BY]-(person0_1)
    //     MERGE (person0_3:Person { Name: $cp_0_0_3 })
    //     CREATE (a_0_0)<-[:ACTED_IN]-(person0_3)
    //     MERGE (person0_5:Person { Name: $cp_0_0_4 })
    //     CREATE (a_0_0)<-[:ACTED_IN]-(person0_5)
    //     """);
    //     sut.CreateCommands.Should().HaveCount(1);
    //     var createCommand = sut.CreateCommands.Single();
    //     createCommand.Parameters.Should().HaveCount(5);
    //     createCommand.Parameters["cp_0_0_0"].Should().Be("The Matrix");
    //     createCommand.Parameters["cp_0_0_1"].Should().Be(1999);
    //     createCommand.Parameters["cp_0_0_2"].Should().Be("The Wachowskis");
    //     createCommand.Parameters["cp_0_0_3"].Should().Be("Keanu Reeves");
    //     createCommand.Parameters["cp_0_0_4"].Should().Be("Laurence Fishburne");
    // }
}