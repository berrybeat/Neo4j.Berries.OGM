using System.Text;
using FluentAssertions;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.General;
using Neo4j.Berries.OGM.Tests.Common;

namespace Neo4j.Berries.OGM.Tests.Models.General;

public class NodeTests : TestBase
{
    [Fact]
    public void Should_Consider_Properties_And_Identifiers()
    {
        var node = new Node("Person");
        node.Consider([
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "John" } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
        ]);
        node.Properties.Should().HaveCount(2);
        node.Properties.Should().Contain("FirstName", "LastName");
        node.Properties.Should().NotContain("Id");
        node.Identifiers.Should().HaveCount(1);
        node.Identifiers.Should().ContainKey("Id");
    }
    [Fact]
    public void Should_Consider_Single_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "John" }, { "LastName", "Doe" }, { "Address", new Dictionary<string, object> { { "City", "Berlin" }, { "Country", "Germany" } } } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" }, { "Address", new Dictionary<string, object> { { "City", "Frankfurt" }, { "Country", "Germany" }, { "AddressLine", "Street 1" } } } },
        ]);
        node.SingleRelations.Should().HaveCount(1);
        var relation = node.SingleRelations["Address"];
        relation.Properties.Should().HaveCount(3);
        relation.Properties.Should().Contain("City", "Country", "AddressLine");

    }
    [Fact]
    public void Should_Consider_Multiple_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                 {
                    "Id", Guid.NewGuid().ToString() },
                    { "FirstName", "John" },
                    { "LastName", "Doe" },
                    { "MoviesAsActor", new List<Dictionary<string, object>> {
                        new () { { "Name", "Movie 1" } },
                        new () { { "Name", "Movie 2" } },
                        new () { { "Name", "Movie 2" }, {"ReleaseDate", new DateTime(1990, 05, 10) } },
                    } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" },
                { "LastName", "Doe" } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" },
                { "LastName", "Doe" },
                { "MoviesAsDirector",
                    new List<Dictionary<string, object>> {
                        new () { { "Name", "Movie 1" } } } } },
        ]);
        node.MultipleRelations.Should().HaveCount(2);
        var actorRelation = node.MultipleRelations["MoviesAsActor"];
        actorRelation.Properties.Should().HaveCount(2);
        actorRelation.Properties.Should().Contain("Name");
        actorRelation.Properties.Should().Contain("ReleaseDate");

        var directorRelation = node.MultipleRelations["MoviesAsDirector"];
        directorRelation.Properties.Should().HaveCount(1);
        directorRelation.Properties.Should().Contain("Name");
    }

    [Fact]
    public void Should_Add_Into_Group_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "LastName", "Doe" },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new List<Dictionary<string, object>> {
                            new () { { "LicensePlate", "AB123" } },
                            new () { { "LicensePlate", "ES123" } },
                        }
                    }
                }}
            }
        ]);
        node.GroupRelations.Should().HaveCount(1);
        node.GroupRelations["Resources"].Should().HaveCount(2);
        node.GroupRelations["Resources"].Should().ContainKey("Room");
        node.GroupRelations["Resources"].Should().ContainKey("Car");

        var room = node.GroupRelations["Resources"]["Room"];
        room.Identifiers.Should().HaveCount(1);
        room.Identifiers.Should().ContainKey("Number");

        var car = node.GroupRelations["Resources"]["Car"];
        car.Identifiers.Should().HaveCount(1);
        car.Identifiers.Should().ContainKey("LicensePlate");

        node.SingleRelations.Should().HaveCount(0);
    }

    [Fact]
    public void Should_Save_Identifier_Values()
    {
        var node = new Node("Person");
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();
        node.Consider([
            new () { { "Id", guid1.ToString() }, { "FirstName", "John" } },
            new () { { "Id", guid2.ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
        ]);
        node.Identifiers.Should().HaveCount(1);
        node.Identifiers.Should().ContainKey("Id");
        node.Identifiers["Id"].Should().HaveCount(2);
        node.Identifiers["Id"].Should().Contain(guid1, guid2);
    }

    [Fact]
    public void Should_Throw_InvalidOperationException_When_Some_Group_Items_Are_Not_Collections()
    {
        var node = new Node("Person");
        var act = () => node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "LastName", "Doe" },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new Dictionary<string, object> {
                            { "LicensePlate", "AB123" },
                            { "Brand", "BMW" }
                        }
                    }
                }}
            }
        ]);
        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("Group items should be collections.");
    }

    [Fact]
    public void Should_Create_A_Simple_Merge()
    {
        var node = new Node("Person");
        node.Consider([
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "John" } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();

        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName, m_0.LastName=muv_0.LastName
        """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Create_A_Cypher_With_Merging_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "MoviesAsActor", new List<Dictionary<string, object>> {
                    new () { { "Name", "Movie 1" } },
                    new () { { "Name", "Movie 2" } },
                    new () { { "Name", "Movie 2" }, {"ReleaseDate", new DateTime(1990, 05, 10) } },
                } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();

        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName
        FOREACH (muv_0_1_0 IN muv_0.MoviesAsActor |
        MERGE (m_0_1_0:Movie) SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
        MERGE (m_0)-[:ACTED_IN]->(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }
    [Fact]
    public void Should_Create_Nested_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "MoviesAsActor", new List<Dictionary<string, object>> {
                    new () { { "Name", "Movie 1" } },
                    new () { { "Name", "Movie 2" } },
                    new () {
                        { "Name", "Movie 2" },
                        {"ReleaseDate", new DateTime(1990, 05, 10) },
                        { "Actors", new List<Dictionary<string, object>> {
                            new () { { "FirstName", "Jake" } },
                            new () { { "FirstName", "John" } },
                        } } },
                } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName
        FOREACH (muv_0_1_0 IN muv_0.MoviesAsActor |
        MERGE (m_0_1_0:Movie) SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
        FOREACH (muv_0_2_0 IN muv_0_1_0.Actors |
        MERGE (m_0_2_0:Person) SET m_0_2_0.FirstName=muv_0_2_0.FirstName
        MERGE (m_0_1_0)<-[:ACTED_IN]-(m_0_2_0)
        )
        MERGE (m_0)-[:ACTED_IN]->(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }
    [Fact]
    public void Should_Create_Cypher_With_Simple_Create()
    {
        var node = new Node("Person");
        node.Consider([
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "John" } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
        ]);
        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();

        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS cuv_0
        CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.LastName=cuv_0.LastName
        """.NormalizeWhitespace());
    }
    [Fact]
    public void Should_Create_Cypher_With_Creating_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "MoviesAsActor", new List<Dictionary<string, object>> {
                    new () { { "Name", "Movie 1" } },
                    new () { { "Name", "Movie 2" } },
                    new () { { "Name", "Movie 2" }, {"ReleaseDate", new DateTime(1990, 05, 10) } },
                } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();

        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS cuv_0
        CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName
        FOREACH (muv_0_1_0 IN cuv_0.MoviesAsActor |
        MERGE (m_0_1_0:Movie) SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
        CREATE (c_0)-[:ACTED_IN]->(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Create_Merge_Cypher_With_Simple_Single_Relation_On_First_Depth()
    {
        var node = new Node("Movie");
        node.Consider([
            new () { { "Name", "The Matrix" }, { "ReleaseDate", new DateTime(1999, 3, 31) } },
            new () {
                { "Name", "The Matrix Reloaded" },
                { "ReleaseDate", new DateTime(2003, 5, 15) },
                { "Director", new Dictionary<string, object> {
                    { "FirstName", "Lana" },
                    { "LastName", "Wachowski" }
                } } },
        ]);

        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$movies", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $movies AS muv_0
        MERGE (m_0:Movie) SET m_0.Name=muv_0.Name, m_0.ReleaseDate=muv_0.ReleaseDate
        FOREACH (ignored IN CASE WHEN muv_0.Director IS NOT NULL THEN [1] ELSE [] END |
        MERGE (m_0_1_0:Person) SET m_0_1_0.FirstName=muv_0.Director.FirstName, m_0_1_0.LastName=muv_0.Director.LastName
        MERGE (m_0)<-[:DIRECTED]-(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Create_Creation_Cypher_With_Simple_Single_Relation_On_First_Depth()
    {
        var node = new Node("Movie");
        node.Consider([
            new () { { "Name", "The Matrix" }, { "ReleaseDate", new DateTime(1999, 3, 31) } },
            new () {
                { "Name", "The Matrix Reloaded" },
                { "ReleaseDate", new DateTime(2003, 5, 15) },
                { "Director", new Dictionary<string, object> {
                    { "FirstName", "Lana" },
                    { "LastName", "Wachowski" }
                } } },
        ]);

        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$movies", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $movies AS cuv_0
        CREATE (c_0:Movie) SET c_0.Name=cuv_0.Name, c_0.ReleaseDate=cuv_0.ReleaseDate
        FOREACH (ignored IN CASE WHEN cuv_0.Director IS NOT NULL THEN [1] ELSE [] END |
        MERGE (m_0_1_0:Person) SET m_0_1_0.FirstName=cuv_0.Director.FirstName, m_0_1_0.LastName=cuv_0.Director.LastName
        CREATE (c_0)<-[:DIRECTED]-(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }
    [Fact]
    public void Should_Create_Cypher_And_Multiple_Relations_Of_A_SingleRelation()
    {
        var node = new Node("Movie");
        node.Consider([
            new () { { "Name", "The Matrix" }, { "ReleaseDate", new DateTime(1999, 3, 31) } },
            new () {
                { "Name", "The Matrix Reloaded" },
                { "ReleaseDate", new DateTime(2003, 5, 15) },
                { "Director", new Dictionary<string, object> {
                    { "FirstName", "Lana" },
                    { "LastName", "Wachowski" },
                    { "MoviesAsActor", new List<Dictionary<string, object>> {
                        new () { { "Name", "Movie 1" } },
                        new () { { "Name", "Movie 2" } },
                        new () { { "Name", "Movie 2" }, {"ReleaseDate", new DateTime(1990, 05, 10) } },
                    } }
                } } },
        ]);

        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$movies", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $movies AS muv_0
        MERGE (m_0:Movie) SET m_0.Name=muv_0.Name, m_0.ReleaseDate=muv_0.ReleaseDate
        FOREACH (ignored IN CASE WHEN muv_0.Director IS NOT NULL THEN [1] ELSE [] END |
        MERGE (m_0_1_0:Person) SET m_0_1_0.FirstName=muv_0.Director.FirstName, m_0_1_0.LastName=muv_0.Director.LastName
        FOREACH (muv_0_2_0 IN muv_0.Director.MoviesAsActor |
        MERGE (m_0_2_0:Movie) SET m_0_2_0.Name=muv_0_2_0.Name, m_0_2_0.ReleaseDate=muv_0_2_0.ReleaseDate
        MERGE (m_0_1_0)-[:ACTED_IN]->(m_0_2_0)
        )
        MERGE (m_0)<-[:DIRECTED]-(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Create_A_Merge_Cypher_And_Create_Single_Relations_Of_A_MultipleRelation()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "MoviesAsActor", new List<Dictionary<string, object>> {
                    new () { { "Name", "Movie 1" } },
                    new () { { "Name", "Movie 2" } },
                    new () {
                        { "Name", "Movie 2" },
                        {"ReleaseDate", new DateTime(1990, 05, 10) },
                        { "Director", new Dictionary<string, object> {
                            { "FirstName", "Jake" } ,
                        } } },
                } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" }
            }
        ]);

        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName
        FOREACH (muv_0_1_0 IN muv_0.MoviesAsActor |
        MERGE (m_0_1_0:Movie) SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
        FOREACH (ignored IN CASE WHEN muv_0_1_0.Director IS NOT NULL THEN [1] ELSE [] END |
        MERGE (m_0_2_0:Person) SET m_0_2_0.FirstName=muv_0_1_0.Director.FirstName
        MERGE (m_0_1_0)<-[:DIRECTED]-(m_0_2_0)
        )
        MERGE (m_0)-[:ACTED_IN]->(m_0_1_0)
        )
        """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Throw_InvalidOperationException_When_Identifiers_Are_Enforced_And_Merge_Is_Applied()
    {
        var node = new Node("Person");
        Neo4jSingletonContext.EnforceIdentifiers = true;
        var act = () => node.Consider([
            new () { { "FirstName", "John" } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
        ]);
        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("Identifiers are enforced but not provided in the data. Label: Person");
    }

    [Fact]
    public void Should_Create_Cypher_For_Group_Relations()
    {
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new List<Dictionary<string, object>> {
                            new () { { "LicensePlate", "AB123" }, { "Brand", "BMW" } },
                            new () { { "LicensePlate", "ES123" } },
                        }
                    }
                }}
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName
        FOREACH (ignored IN CASE WHEN muv_0.Resources IS NOT NULL THEN [1] ELSE [] END |
        FOREACH (muv_0_1_0 IN muv_0.Resources.Room |
        MERGE (m_0_1_0:Room {Number: muv_0_1_0.Number})
        MERGE (m_0)-[:USES]->(m_0_1_0)
        )
        FOREACH (muv_0_1_0 IN muv_0.Resources.Car |
        MERGE (m_0_1_0:Car {LicensePlate: muv_0_1_0.LicensePlate}) SET m_0_1_0.Brand=muv_0_1_0.Brand
        MERGE (m_0)-[:USES]->(m_0_1_0)
        )
        )
        """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Extend_Cypher_With_CreatedOn_And_ModifiedOn_For_Nodes()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id})
        ON CREATE SET m_0.createdOn=timestamp()
        ON MATCH SET m_0.modifiedOn=timestamp()
        SET m_0.FirstName=muv_0.FirstName
        """.NormalizeWhitespace());
    }
    [Fact]
    public void Should_Extend_Cypher_With_ModifiedOn_For_Nodes_OnCreation_And_OnMatch()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        sut.NormalizeWhitespace().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id})
        ON CREATE SET m_0.createdOn=timestamp(), m_0.modifiedOn=timestamp()
        ON MATCH SET m_0.modifiedOn=timestamp()
        SET m_0.FirstName=muv_0.FirstName
        """.NormalizeWhitespace());
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Should_Extend_Cypher_With_CreatedOn_For_Nodes_On_Add(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        if (enforceModifiedTimestampKey)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS cuv_0
            CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.createdOn=timestamp(), c_0.modifiedOn=timestamp()
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS cuv_0
            CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.createdOn=timestamp()
            """.NormalizeWhitespace());
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Extend_Cypher_With_ModifiedOn_And_CreatedOn_On_Merging_Relations(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;
        var node = new Node("Movie");
        node.Consider([
            new () { { "Name", "The Matrix" }, { "ReleaseDate", new DateTime(1999, 3, 31) } },
            new () {
                { "Name", "The Matrix Reloaded" },
                { "ReleaseDate", new DateTime(2003, 5, 15) },
                { "Director", new Dictionary<string, object> {
                    { "FirstName", "Lana" },
                    { "LastName", "Wachowski" }
                } } },
        ]);

        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$movies", 0);
        var sut = cypherBuilder.ToString();
        if (!enforceModifiedTimestampKey)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $movies AS muv_0
            MERGE (m_0:Movie)
            ON CREATE SET m_0.createdOn=timestamp()
            ON MATCH SET m_0.modifiedOn=timestamp()
            SET m_0.Name=muv_0.Name, m_0.ReleaseDate=muv_0.ReleaseDate
            FOREACH (ignored IN CASE WHEN muv_0.Director IS NOT NULL THEN [1] ELSE [] END |
            MERGE (m_0_1_0:Person)
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.FirstName=muv_0.Director.FirstName, m_0_1_0.LastName=muv_0.Director.LastName
            MERGE (m_0)<-[r_0:DIRECTED]-(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $movies AS muv_0
            MERGE (m_0:Movie)
            ON CREATE SET m_0.createdOn=timestamp(), m_0.modifiedOn=timestamp()
            ON MATCH SET m_0.modifiedOn=timestamp()
            SET m_0.Name=muv_0.Name, m_0.ReleaseDate=muv_0.ReleaseDate
            FOREACH (ignored IN CASE WHEN muv_0.Director IS NOT NULL THEN [1] ELSE [] END |
            MERGE (m_0_1_0:Person)
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.FirstName=muv_0.Director.FirstName, m_0_1_0.LastName=muv_0.Director.LastName
            MERGE (m_0)<-[r_0:DIRECTED]-(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            """.NormalizeWhitespace());
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void On_Creating_Single_Relation_Should_Only_Use_Set_And_No_OnMatch_Or_OnCreate(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;
        var node = new Node("Movie");
        node.Consider([
            new () { { "Name", "The Matrix" }, { "ReleaseDate", new DateTime(1999, 3, 31) } },
            new () {
                { "Name", "The Matrix Reloaded" },
                { "ReleaseDate", new DateTime(2003, 5, 15) },
                { "Director", new Dictionary<string, object> {
                    { "FirstName", "Lana" },
                    { "LastName", "Wachowski" }
                } } },
        ]);

        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$movies", 0);
        var sut = cypherBuilder.ToString();
        if (enforceModifiedTimestampKey == false)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $movies AS cuv_0
            CREATE (c_0:Movie) SET c_0.Name=cuv_0.Name, c_0.ReleaseDate=cuv_0.ReleaseDate, c_0.createdOn=timestamp()
            FOREACH (ignored IN CASE WHEN cuv_0.Director IS NOT NULL THEN [1] ELSE [] END |
            MERGE (m_0_1_0:Person)
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.FirstName=cuv_0.Director.FirstName, m_0_1_0.LastName=cuv_0.Director.LastName
            CREATE (c_0)<-[r_0:DIRECTED]-(m_0_1_0)
            SET r_0.createdOn=timestamp()
            )
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $movies AS cuv_0
            CREATE (c_0:Movie) SET c_0.Name=cuv_0.Name, c_0.ReleaseDate=cuv_0.ReleaseDate, c_0.createdOn=timestamp(), c_0.modifiedOn=timestamp()
            FOREACH (ignored IN CASE WHEN cuv_0.Director IS NOT NULL THEN [1] ELSE [] END |
            MERGE (m_0_1_0:Person)
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.FirstName=cuv_0.Director.FirstName, m_0_1_0.LastName=cuv_0.Director.LastName
            CREATE (c_0)<-[r_0:DIRECTED]-(m_0_1_0)
            SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            )
            """.NormalizeWhitespace());
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void On_Merging_Multiple_Relations_Timestamps_Should_Be_Added(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "MoviesAsActor", new List<Dictionary<string, object>> {
                    new () { { "Name", "Movie 1" } },
                    new () { { "Name", "Movie 2" } },
                    new () { { "Name", "Movie 2" }, {"ReleaseDate", new DateTime(1990, 05, 10) } },
                } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        if (enforceModifiedTimestampKey == false)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS muv_0
            MERGE (m_0:Person {Id: muv_0.Id})
            ON CREATE SET m_0.createdOn=timestamp()
            ON MATCH SET m_0.modifiedOn=timestamp()
            SET m_0.FirstName=muv_0.FirstName
            FOREACH (muv_0_1_0 IN muv_0.MoviesAsActor |
            MERGE (m_0_1_0:Movie)
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
            MERGE (m_0)-[r_0:ACTED_IN]->(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS muv_0
            MERGE (m_0:Person {Id: muv_0.Id})
            ON CREATE SET m_0.createdOn=timestamp(), m_0.modifiedOn=timestamp()
            ON MATCH SET m_0.modifiedOn=timestamp()
            SET m_0.FirstName=muv_0.FirstName
            FOREACH (muv_0_1_0 IN muv_0.MoviesAsActor |
            MERGE (m_0_1_0:Movie)
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
            MERGE (m_0)-[r_0:ACTED_IN]->(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            """.NormalizeWhitespace());
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void On_Creating_Multi_Relations_Timestamps_Should_Be_Added(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "MoviesAsActor", new List<Dictionary<string, object>> {
                    new () { { "Name", "Movie 1" } },
                    new () { { "Name", "Movie 2" } },
                    new () { { "Name", "Movie 2" }, {"ReleaseDate", new DateTime(1990, 05, 10) } },
                } } },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        if (enforceModifiedTimestampKey == false)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS cuv_0
            CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.createdOn=timestamp()
            FOREACH (muv_0_1_0 IN cuv_0.MoviesAsActor |
            MERGE (m_0_1_0:Movie)
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
            CREATE (c_0)-[r_0:ACTED_IN]->(m_0_1_0)
            SET r_0.createdOn=timestamp()
            )
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS cuv_0
            CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.createdOn=timestamp(), c_0.modifiedOn=timestamp()
            FOREACH (muv_0_1_0 IN cuv_0.MoviesAsActor |
            MERGE (m_0_1_0:Movie)
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
            CREATE (c_0)-[r_0:ACTED_IN]->(m_0_1_0)
            SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            )
            """.NormalizeWhitespace());
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void On_Merging_Group_Relations_Timestamps_Should_Be_Added(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;

        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new List<Dictionary<string, object>> {
                            new () { { "LicensePlate", "AB123" }, { "Brand", "BMW" } },
                            new () { { "LicensePlate", "ES123" } },
                        }
                    }
                }}
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Merge(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        if (enforceModifiedTimestampKey == false)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS muv_0
            MERGE (m_0:Person {Id: muv_0.Id})
            ON CREATE SET m_0.createdOn=timestamp()
            ON MATCH SET m_0.modifiedOn=timestamp()
            SET m_0.FirstName=muv_0.FirstName
            FOREACH (ignored IN CASE WHEN muv_0.Resources IS NOT NULL THEN [1] ELSE [] END |
            FOREACH (muv_0_1_0 IN muv_0.Resources.Room |
            MERGE (m_0_1_0:Room {Number: muv_0_1_0.Number})
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            MERGE (m_0)-[r_0:USES]->(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            FOREACH (muv_0_1_0 IN muv_0.Resources.Car |
            MERGE (m_0_1_0:Car {LicensePlate: muv_0_1_0.LicensePlate})
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Brand=muv_0_1_0.Brand
            MERGE (m_0)-[r_0:USES]->(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            )
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS muv_0
            MERGE (m_0:Person {Id: muv_0.Id})
            ON CREATE SET m_0.createdOn=timestamp(), m_0.modifiedOn=timestamp()
            ON MATCH SET m_0.modifiedOn=timestamp()
            SET m_0.FirstName=muv_0.FirstName
            FOREACH (ignored IN CASE WHEN muv_0.Resources IS NOT NULL THEN [1] ELSE [] END |
            FOREACH (muv_0_1_0 IN muv_0.Resources.Room |
            MERGE (m_0_1_0:Room {Number: muv_0_1_0.Number})
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            MERGE (m_0)-[r_0:USES]->(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            FOREACH (muv_0_1_0 IN muv_0.Resources.Car |
            MERGE (m_0_1_0:Car {LicensePlate: muv_0_1_0.LicensePlate})
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Brand=muv_0_1_0.Brand
            MERGE (m_0)-[r_0:USES]->(m_0_1_0)
            ON CREATE SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            ON MATCH SET r_0.modifiedOn=timestamp(), r_0.archivedOn=null
            )
            )
            """.NormalizeWhitespace());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void On_Creating_Group_Relations_Timestamps_Should_Be_Added(bool enforceModifiedTimestampKey)
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.TimestampConfiguration.EnforceModifiedTimestampKey = enforceModifiedTimestampKey;

        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new List<Dictionary<string, object>> {
                            new () { { "LicensePlate", "AB123" }, { "Brand", "BMW" } },
                            new () { { "LicensePlate", "ES123" } },
                        }
                    }
                }}
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();
        if (enforceModifiedTimestampKey == false)
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS cuv_0
            CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.createdOn=timestamp()
            FOREACH (ignored IN CASE WHEN cuv_0.Resources IS NOT NULL THEN [1] ELSE [] END |
            FOREACH (muv_0_1_0 IN cuv_0.Resources.Room |
            MERGE (m_0_1_0:Room {Number: muv_0_1_0.Number})
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            CREATE (c_0)-[r_0:USES]->(m_0_1_0)
            SET r_0.createdOn=timestamp()
            )
            FOREACH (muv_0_1_0 IN cuv_0.Resources.Car |
            MERGE (m_0_1_0:Car {LicensePlate: muv_0_1_0.LicensePlate})
            ON CREATE SET m_0_1_0.createdOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Brand=muv_0_1_0.Brand
            CREATE (c_0)-[r_0:USES]->(m_0_1_0)
            SET r_0.createdOn=timestamp()
            )
            )
            """.NormalizeWhitespace());
        else
            sut.NormalizeWhitespace().Should().Be("""
            UNWIND $people AS cuv_0
            CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.createdOn=timestamp(), c_0.modifiedOn=timestamp()
            FOREACH (ignored IN CASE WHEN cuv_0.Resources IS NOT NULL THEN [1] ELSE [] END |
            FOREACH (muv_0_1_0 IN cuv_0.Resources.Room |
            MERGE (m_0_1_0:Room {Number: muv_0_1_0.Number})
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            CREATE (c_0)-[r_0:USES]->(m_0_1_0)
            SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            )
            FOREACH (muv_0_1_0 IN cuv_0.Resources.Car |
            MERGE (m_0_1_0:Car {LicensePlate: muv_0_1_0.LicensePlate})
            ON CREATE SET m_0_1_0.createdOn=timestamp(), m_0_1_0.modifiedOn=timestamp()
            ON MATCH SET m_0_1_0.modifiedOn=timestamp()
            SET m_0_1_0.Brand=muv_0_1_0.Brand
            CREATE (c_0)-[r_0:USES]->(m_0_1_0)
            SET r_0.createdOn=timestamp(), r_0.modifiedOn=timestamp()
            )
            )
            """.NormalizeWhitespace());
    }

    [Fact]
    public void Should_Archive_All_Root_Relations_If_Marked_As_KeepHistory()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Address"].KeepHistory = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Address", new Dictionary<string, object> {
                    { "Street", "Street 1" },
                } }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.ArchiveRelations(cypherBuilder, 0, out var variables);
        var cypher = cypherBuilder.ToString().Trim();
        cypher.NormalizeWhitespace().Should().Be("""
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:LIVES_IN WHERE r_0.archivedOn IS null]->(:Address) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        """.NormalizeWhitespace());
        variables.Should().ContainKey("person_id_0");
        var identifiers = variables["person_id_0"] as List<object>;
        identifiers.Should().HaveCount(1);
        identifiers.Should().Contain(node.Identifiers["Id"]);
    }

    [Fact]
    public void Should_Archive_All_Multiple_Relations()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Friends"].KeepHistory = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Friends", new List<Dictionary<string, object>> {
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" } },
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jane" } },
                } }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.ArchiveRelations(cypherBuilder, 0, out var variables);
        var cypher = cypherBuilder.ToString().Trim();
        cypher.NormalizeWhitespace().Should().Be("""
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:FRIENDS_WITH WHERE r_0.archivedOn IS null]->(:Person) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        """.NormalizeWhitespace());
        variables.Should().ContainKey("person_id_0");
        variables.Should().HaveCount(1);
        var identifiers = variables["person_id_0"] as List<object>;
        identifiers.Should().HaveCount(1);
        identifiers.Should().Contain(node.Identifiers["Id"]);
    }

    [Fact]
    public void Should_Archive_Group_Relations()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Resources"].KeepHistory = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new List<Dictionary<string, object>> {
                            new () { { "LicensePlate", "AB123" }, { "Brand", "BMW" } },
                            new () { { "LicensePlate", "ES123" } },
                        }
                    }
                }}
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.ArchiveRelations(cypherBuilder, 0, out var variables);
        var cypher = cypherBuilder.ToString().Trim();
        cypher.NormalizeWhitespace().Should().Be("""
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:USES WHERE r_0.archivedOn IS null]->(:Car) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:USES WHERE r_0.archivedOn IS null]->(:Room) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        """.NormalizeWhitespace());
        variables.Should().ContainKey("person_id_0");
        variables.Should().HaveCount(1);
        var identifiers = variables["person_id_0"] as List<object>;
        identifiers.Should().HaveCount(1);
        identifiers.Should().Contain(node.Identifiers["Id"]);
    }

    [Fact]
    public void Should_Archive_All_KeepHistory_Relations_In_Root()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Friends"].KeepHistory = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Address"].KeepHistory = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Resources"].KeepHistory = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Address", new Dictionary<string, object> {
                    { "Street", "Street 1" },
                } },
                { "Friends", new List<Dictionary<string, object>> {
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" } },
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jane" } },
                } },
                { "Resources", new Dictionary<string, object> {
                    {
                        "Room",
                        new List<Dictionary<string, object>> {
                            new () { { "Number", "100" } },
                            new () { { "Number", "101" } },
                        }
                    },
                    {
                        "Car",
                        new List<Dictionary<string, object>> {
                            new () { { "LicensePlate", "AB123" }, { "Brand", "BMW" } },
                            new () { { "LicensePlate", "ES123" } },
                        }
                    }
                }}
            },
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "Jake" },
                { "Address", new Dictionary<string, object> {
                    { "Street", "Street 1" },
                } },
                { "Friends", new List<Dictionary<string, object>> {
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jun" } },
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Janet" } },
                } }
            },
        ]);
        var cypherBuilder = new StringBuilder();
        node.ArchiveRelations(cypherBuilder, 0, out var variables);
        var cypher = cypherBuilder.ToString().Trim();
        cypher.NormalizeWhitespace().Should().Be("""
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:LIVES_IN WHERE r_0.archivedOn IS null]->(:Address) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:FRIENDS_WITH WHERE r_0.archivedOn IS null]->(:Person) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:USES WHERE r_0.archivedOn IS null]->(:Car) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        OPTIONAL MATCH(a_0:Person WHERE a_0.Id IN $person_id_0)-[r_0:USES WHERE r_0.archivedOn IS null]->(:Room) SET r_0.archivedOn=timestamp()
        WITH 0 AS nothing
        """.NormalizeWhitespace());
        variables.Should().ContainKey("person_id_0");
        variables.Should().HaveCount(1);
        var identifiers = variables["person_id_0"] as List<object>;
        identifiers.Should().HaveCount(2);
        identifiers.Should().Contain(node.Identifiers["Id"]);
    }

    [Fact]
    public void Should_Not_Set_Variables_If_No_Node_Has_KeepHistory_Flag() {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Friends", new List<Dictionary<string, object>> {
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "Address", new Dictionary<string, object> {
                        { "Street", "Street 1" }
                    } } },
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jane" } },
                } }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.ArchiveRelations(cypherBuilder, 0, out var variables);
        variables.Should().BeEmpty();
        cypherBuilder.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Should_Archive_Nested_Single_Relations()
    {
        Neo4jSingletonContext.TimestampConfiguration.Enabled = true;
        Neo4jSingletonContext.Configs["Person"].Relations["Address"].KeepHistory = true;
        var node = new Node("Person");
        node.Consider([
            new () {
                { "Id", Guid.NewGuid().ToString() },
                { "FirstName", "John" },
                { "Friends", new List<Dictionary<string, object>> {
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "Address", new Dictionary<string, object> {
                        { "Street", "Street 1" }
                    } } },
                    new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jane" } },
                } }
            }
        ]);
        var cypherBuilder = new StringBuilder();
        node.ArchiveRelations(cypherBuilder, 0, out var variables);
        var cypher = cypherBuilder.ToString().Trim();
        cypher.NormalizeWhitespace().Should().Be("""
        OPTIONAL MATCH(a_0_1_0:Person WHERE a_0_1_0.Id IN $person_id_1)-[r_0_1_0:LIVES_IN WHERE r_0_1_0.archivedOn IS null]->(:Address) SET r_0_1_0.archivedOn=timestamp()
        WITH 0 AS nothing
        """.NormalizeWhitespace());
        variables.Should().ContainKey("person_id_1");
        variables.Should().HaveCount(1);
        var identifiers = variables["person_id_1"] as List<object>;
        identifiers.Should().HaveCount(2);
        identifiers.Should().Contain(node.MultipleRelations["Friends"].Identifiers["Id"]);
    }

}