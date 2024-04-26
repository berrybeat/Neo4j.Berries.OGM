using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using FluentAssertions;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Neo4j.Berries.OGM.Tests.Models;


public class CreateCommandTests
{
    public StringBuilder CypherBuilder { get; }

    public CreateCommandTests()
    {
        _ = new Neo4jSingletonContext(GetType().Assembly);
        CypherBuilder = new StringBuilder();
    }
    [Fact]
    public void Should_Create_Simple_Create_Command()
    {
        var sut = GetSUTInstance(new(), "movie_0");
        sut.Add(new() {
            { "Id", Guid.NewGuid() },
            { "Title", "The Matrix" },
            { "Released", 1999 }
        });
        sut.GenerateCypher("Movie");
        var cypher = CypherBuilder.ToString();
        cypher.Trim().Should().Be("CREATE (node_0:Movie { Id: movie_0.Id, Title: movie_0.Title, Released: movie_0.Released })");
    }
    [Fact]
    public void Should_Create_Command_With_Merging_Single_Relations()
    {
        var nodeConfig = new NodeConfiguration();
        var relationConfig = new RelationConfiguration("Person", "DIRECTED", RelationDirection.In);
        nodeConfig.Relations.TryAdd("Director", relationConfig);

        var sut = GetSUTInstance(nodeConfig, "movie_0");
        sut.Add(new() {
            { "Id", Guid.NewGuid() },
            { "Title", "The Matrix" },
            { "Released", 1999 },
            { "Director", new Dictionary<string, object> {
                { "Name", "Lana Wachowski" },
                { "Born", 1965 }
            } }
        });
        sut.GenerateCypher("Movie");
        var cypher = CypherBuilder.ToString();
        cypher.Trim().Should()
            .Be("""
            CREATE (node_0:Movie { Id: movie_0.Id, Title: movie_0.Title, Released: movie_0.Released })
            WITH *
            WHERE movie_0.Director IS NOT NULL
            MERGE (director_0:Person { Name: movie_0.Director.Name, Born: movie_0.Director.Born })
            CREATE (node_0)<-[:DIRECTED]-(director_0)
            """);
    }
    [Fact]
    public void Should_Take_Merge_Props_Into_Account()
    {
        var nodeConfig = Neo4jSingletonContext.Configs["Movie"];
        var sut = GetSUTInstance(nodeConfig, "movie_0");
        sut.Add(new Movie
        {
            Id = Guid.NewGuid(),
            Name = "The Matrix",
            ReleaseDate = new DateTime(1999, 3, 31),
            Director = new Person
            {
                FirstName = "Lana",
                LastName = "Wachowski",
                MoviesAsDirector = [
                    new Movie() {
                        Id = Guid.NewGuid(),
                        Name = "The Matrix Reloaded",
                        ReleaseDate = new DateTime(2003, 5, 15),
                        Actors = [
                            new Person {
                                Id = Guid.NewGuid(),
                                FirstName = "Carrie-Anne",
                                LastName = "Moss",
                            }
                        ]
                    }
                ]
            },
            Actors = [
                new Person {
                    Id = Guid.NewGuid(),
                    FirstName = "Keanu",
                    LastName = "Reeves",
                    Age = 56
                },
                new Person {
                    Id = Guid.NewGuid(),
                    FirstName = "Carrie-Anne",
                    LastName = "Moss",
                }
            ]
        });
        sut.GenerateCypher("Movie");
        var cypher = CypherBuilder.ToString();
        cypher.Trim().Should().Be("""
        CREATE (node_0:Movie { Id: movie_0.Id, Name: movie_0.Name, ReleaseDate: movie_0.ReleaseDate })
        WITH *
        WHERE movie_0.Director IS NOT NULL
        MERGE (director_0:Person { Id: movie_0.Director.Id })
        CREATE (node_0)<-[:DIRECTED]-(director_0)
        WITH *
        WHERE movie_0.Actors IS NOT NULL
        UNWIND movie_0.Actors AS uw_actors_0
        MERGE (actors_0:Person { Id: uw_actors_0.Id })
        CREATE (node_0)<-[:ACTED_IN]-(actors_0)
        """);
    }
    [Fact]
    public void Should_Add_Set_Command_For_Nullable_Properties_In_Relations()
    {
        var nodeConfig = new NodeConfiguration();
        var directedRelationConfig = new RelationConfiguration("Person", "DIRECTED", RelationDirection.In);
        var actedRelationConfig = new RelationConfiguration("Person", "ACTED_IN", RelationDirection.In);
        nodeConfig.Relations.TryAdd("Director", directedRelationConfig);
        nodeConfig.Relations.TryAdd("Actors", actedRelationConfig);
        var sut = GetSUTInstance(nodeConfig, "movie_0");
        sut.Add(new Dictionary<string, object>
        {
            {"Id", Guid.NewGuid()},
            {"Name", "The Matrix"},
            {"ReleaseDate", new DateTime(1999, 3, 31)},
            {"Director", new Dictionary<string, object>
            {
                {"FirstName", "Lana"},
                {"LastName", "Wachowski"},
            }},
            {"Actors", new List<Dictionary<string, object>> {
                new () {
                    {"Id", Guid.NewGuid()},
                    {"FirstName", "Keanu"},
                    {"LastName", "Reeves"},
                    {"Age", 56}
                },
                new () {
                    {"Id", Guid.NewGuid()},
                    {"FirstName", "Carrie-Anne"},
                    {"LastName", "Moss"},
                }
            }}
        });
        sut.GenerateCypher("Movie");
        var cypher = CypherBuilder.ToString();
        cypher.Trim().Should().Be("""
        CREATE (node_0:Movie { Id: movie_0.Id, Name: movie_0.Name, ReleaseDate: movie_0.ReleaseDate })
        WITH *
        WHERE movie_0.Director IS NOT NULL
        MERGE (director_0:Person { FirstName: movie_0.Director.FirstName, LastName: movie_0.Director.LastName })
        CREATE (node_0)<-[:DIRECTED]-(director_0)
        WITH *
        WHERE movie_0.Actors IS NOT NULL
        UNWIND movie_0.Actors AS uw_actors_0
        MERGE (actors_0:Person { Id: uw_actors_0.Id, FirstName: uw_actors_0.FirstName, LastName: uw_actors_0.LastName })
        SET actors_0.Age = uw_actors_0.Age
        CREATE (node_0)<-[:ACTED_IN]-(actors_0)
        """);
    }
    [Fact]
    public void Should_Add_Set_Command_For_Nullable_Properties_In_Relations_SingleRelations()
    {
        var nodeConfig = new NodeConfiguration();
        var directedRelationConfig = new RelationConfiguration("Person", "DIRECTED", RelationDirection.In);
        var actedRelationConfig = new RelationConfiguration("Person", "ACTED_IN", RelationDirection.In);
        nodeConfig.Relations.TryAdd("Director", directedRelationConfig);
        nodeConfig.Relations.TryAdd("Actors", actedRelationConfig);
        var sut = GetSUTInstance(nodeConfig, "movie_0");
        sut.Add(new Dictionary<string, object>
        {
            {"Id", Guid.NewGuid()},
            {"Name", "The Matrix"},
            {"ReleaseDate", new DateTime(1999, 3, 31)},
            {"Director", new Dictionary<string, object>
            {
                {"FirstName", "Lana"},
                {"LastName", "Wachowski"},
            }}
        });
        sut.Add(new Dictionary<string, object> {
            {"Id", Guid.NewGuid()},
            { "Name", "The Matrix Reloaded" },
            {"Director", new Dictionary<string, object> {
                {"FirstName", "Lana"},
                {"LastName", "Wachowski"},
                { "Age", 56 }
            }}
        });

        sut.GenerateCypher("Movie");
        var cypher = CypherBuilder.ToString();
        cypher.Trim().Should().Be("""
        CREATE (node_0:Movie { Id: movie_0.Id, Name: movie_0.Name, ReleaseDate: movie_0.ReleaseDate })
        WITH *
        WHERE movie_0.Director IS NOT NULL
        MERGE (director_0:Person { FirstName: movie_0.Director.FirstName, LastName: movie_0.Director.LastName })
        SET director_0.Age = movie_0.Director.Age
        CREATE (node_0)<-[:DIRECTED]-(director_0)
        """);
    }
    private CreateCommand GetSUTInstance(NodeConfiguration nodeConfiguration, string unwindVariable)
    {
        var sut = new CreateCommand(0, unwindVariable, nodeConfiguration, CypherBuilder);
        return sut;
    }
}