using System.Diagnostics;
using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models;
using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using FluentAssertions;
using Neo4j.Berries.OGM.Tests.Contexts;
using System.Diagnostics.Contracts;
using Bogus.DataSets;


namespace Neo4j.Berries.OGM.Tests.Models;


public class CreateCommandTests
{
    public StringBuilder CypherBuilder { get; }

    public CreateCommandTests()
    {
        _ = new Neo4jSingletonContext(this.GetType().Assembly);
        CypherBuilder = new StringBuilder();
    }
    [Fact]
    public void Build_Parameters_BasedOnGivenIndex()
    {
        var index = 0;
        var nodeSetIndex = 0;
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix"
        };
        var sut = new CreateCommand<Movie>(movie, index, nodeSetIndex, CypherBuilder);
        sut.Parameters.Should().HaveCount(3);
        sut.Parameters[$"cp_{nodeSetIndex}_{index}_0"].Should().Be(movie.Id.ToString());
        sut.Parameters[$"cp_{nodeSetIndex}_{index}_1"].Should().Be(movie.Name);
        sut.Parameters[$"cp_{nodeSetIndex}_{index}_2"].Should().Be(movie.Year);
    }
    [Fact]
    public void Build_Cypher_BasedOnGivenIndex()
    {
        var index = 0;
        var nodeSetIndex = 0;
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix"
        };
        _ = new CreateCommand<Movie>(movie, index, nodeSetIndex, CypherBuilder);
        CypherBuilder.ToString().Should().Be($"CREATE (movie0:Movie {{ Id: $cp_{nodeSetIndex}_{index}_0, Name: $cp_{nodeSetIndex}_{index}_1, Year: $cp_{nodeSetIndex}_{index}_2 }})\n");
    }
    [Fact]
    public void Parameters_With_GuidType_Should_Be_Converted_ToString()
    {
        var index = 0;
        var nodeSetIndex = 0;
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix"
        };
        var sut = new CreateCommand<Movie>(movie, index, nodeSetIndex, CypherBuilder);
        sut.Parameters[$"cp_{nodeSetIndex}_{index}_0"].Should().BeOfType<string>();
    }
    [Fact]
    public void Should_Create_Cypher_With_Creating_One_Relation()
    {
        var index = 0;
        var nodeSetIndex = 0;
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            Year = 1999,
            Director = new Person
            {
                Id = Guid.NewGuid(),
            }
        };
        var sut = new CreateCommand<Movie>(movie, index, nodeSetIndex, CypherBuilder);

        CypherBuilder.ToString().Trim().Should().Be("""
        CREATE (movie0:Movie { Id: $cp_0_0_0, Name: $cp_0_0_1, Year: $cp_0_0_2 })
        MERGE (person0_1:Person { Id: $cp_0_0_3 })
        CREATE (movie0)<-[:DIRECTED]-(person0_1)
        """);
        sut.Parameters["cp_0_0_3"].Should().Be(movie.Director.Id.ToString());
    }
    [Fact]
    public void Should_Create_Cypher_With_Creating_A_Collection_Of_Relations()
    {
        var index = 0;
        var nodeSetIndex = 0;
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            Year = 1999,
            Actors =
            [
                new()
                {
                    Id = Guid.NewGuid()
                },
                new()
                {
                    Id = Guid.NewGuid()
                }
            ]
        };
        var sut = new CreateCommand<Movie>(movie, index, nodeSetIndex, CypherBuilder);
        CypherBuilder.ToString().Trim().Should().Be("""
        CREATE (movie0:Movie { Id: $cp_0_0_0, Name: $cp_0_0_1, Year: $cp_0_0_2 })
        MERGE (person0_1:Person { Id: $cp_0_0_3 })
        CREATE (movie0)<-[:ACTED_IN]-(person0_1)
        MERGE (person0_3:Person { Id: $cp_0_0_4 })
        CREATE (movie0)<-[:ACTED_IN]-(person0_3)
        """);
        sut.Parameters["cp_0_0_3"].Should().Be(movie.Actors[0].Id.ToString());
        sut.Parameters["cp_0_0_4"].Should().Be(movie.Actors[1].Id.ToString());
    }
    [Fact]
    public void Should_Take_Less_Than_A_Second_To_Build_1000_Commands()
    {
        var index = 0;
        var nodeSetIndex = 0;
        var movies = FakeMovies.GetMovie(1000); //This means 1000 movies an 10 actors per movie = 10000 actors and 1000 movies = 11000 nodes
        var sw = new Stopwatch();
        sw.Start();
        foreach (var movie in movies)
        {
            _ = new CreateCommand<Movie>(movie, index, nodeSetIndex, CypherBuilder);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }
    [Fact]
    public void Should_Create_Shadow_Node_Without_Having_The_Configuration()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            Year = 1999,
            Location = new Location
            {
                Id = Guid.NewGuid()
            }
        };
        var sut = new CreateCommand<Movie>(movie, 0, 0, CypherBuilder);
        CypherBuilder.ToString().Trim().Should().Be("""
        CREATE (movie0:Movie { Id: $cp_0_0_0, Name: $cp_0_0_1, Year: $cp_0_0_2 })
        MERGE (location0_1:Location { Id: $cp_0_0_3 })
        CREATE (movie0)-[:FILMED_AT]->(location0_1)
        """);
        sut.Parameters["cp_0_0_0"].Should().Be(movie.Id.ToString());
        sut.Parameters["cp_0_0_1"].Should().Be(movie.Name);
        sut.Parameters["cp_0_0_2"].Should().Be(movie.Year);
        sut.Parameters["cp_0_0_3"].Should().Be(movie.Location.Id.ToString());
    }
    [Fact]
    public void Should_Create_Target_Nodes_Without_Configuration()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            Year = 1999,
            Equipments =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Camera Test",
                    Type = Mocks.Enums.EquipmentType.Camera

                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Light Test",
                    Type = Mocks.Enums.EquipmentType.Light
                }
            ]
        };
        var sut = new CreateCommand<Movie>(movie, 0, 0, CypherBuilder);
        CypherBuilder.ToString().Trim().Should().Be("""
        CREATE (movie0:Movie { Id: $cp_0_0_0, Name: $cp_0_0_1, Year: $cp_0_0_2 })
        MERGE (equipment0_1:Equipment { Id: $cp_0_0_3, Name: $cp_0_0_4, Type: $cp_0_0_5 })
        CREATE (movie0)-[:USES]->(equipment0_1)
        MERGE (equipment0_3:Equipment { Id: $cp_0_0_6, Name: $cp_0_0_7, Type: $cp_0_0_8 })
        CREATE (movie0)-[:USES]->(equipment0_3)
        """);
        sut.Parameters["cp_0_0_0"].Should().Be(movie.Id.ToString());
        sut.Parameters["cp_0_0_1"].Should().Be(movie.Name);
        sut.Parameters["cp_0_0_2"].Should().Be(movie.Year);
        sut.Parameters["cp_0_0_3"].Should().Be(movie.Equipments[0].Id.ToString());
        sut.Parameters["cp_0_0_4"].Should().Be(movie.Equipments[0].Name);
        sut.Parameters["cp_0_0_5"].Should().Be(movie.Equipments[0].Type.ToString());

        sut.Parameters["cp_0_0_6"].Should().Be(movie.Equipments[1].Id.ToString());
        sut.Parameters["cp_0_0_7"].Should().Be(movie.Equipments[1].Name);
        sut.Parameters["cp_0_0_8"].Should().Be(movie.Equipments[1].Type.ToString());
    }

    [Fact]
    public void Should_Create_Node_Without_Config() {
        var equipment = new Equipment {
            Id = Guid.NewGuid(),
            Name = "Camera Test",
            Type = Mocks.Enums.EquipmentType.Camera
        };
        var sut = new CreateCommand<Equipment>(equipment, 0, 0, CypherBuilder);
        CypherBuilder.ToString().Trim().Should().Be("""
        CREATE (equipment0:Equipment { Id: $cp_0_0_0, Name: $cp_0_0_1, Type: $cp_0_0_2 })
        """);
        sut.Parameters["cp_0_0_0"].Should().Be(equipment.Id.ToString());
        sut.Parameters["cp_0_0_1"].Should().Be(equipment.Name);
        sut.Parameters["cp_0_0_2"].Should().Be(equipment.Type.ToString());
    }
}