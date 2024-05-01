using System.Text;
using Bogus.DataSets;
using FluentAssertions;
using Neo4j.Berries.OGM.Models.Sets;
using Neo4j.Berries.OGM.Tests.Common;

namespace Neo4j.Berries.OGM.Tests.Models.General;

public class NodeSetTests : TestBase
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
        node.Identifiers.Should().Contain("Id");
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

        sut.Trim().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName, m_0.LastName=muv_0.LastName
        """);
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

        sut.Trim().Should().Be("""
        UNWIND $people AS muv_0
        MERGE (m_0:Person {Id: muv_0.Id}) SET m_0.FirstName=muv_0.FirstName
        FOREACH (muv_0_1_0 IN muv_0.MoviesAsActor |
        MERGE (m_0_1_0:Movie) SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
        MERGE (m_0)-[:ACTED_IN]->(m_0_1_0)
        )
        """);
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
        sut.Trim().Should().Be("""
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
        """);
    }
    [Fact]
    public void Should_Create_Cypher_With_Simple_Create() {
        var node = new Node("Person");
        node.Consider([
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "John" } },
            new () { { "Id", Guid.NewGuid().ToString() }, { "FirstName", "Jake" }, { "LastName", "Doe" } },
        ]);
        var cypherBuilder = new StringBuilder();
        node.Create(cypherBuilder, "$people", 0);
        var sut = cypherBuilder.ToString();

        sut.Trim().Should().Be("""
        UNWIND $people AS cuv_0
        CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName, c_0.LastName=cuv_0.LastName
        """);
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

        sut.Trim().Should().Be("""
        UNWIND $people AS cuv_0
        CREATE (c_0:Person) SET c_0.Id=cuv_0.Id, c_0.FirstName=cuv_0.FirstName
        FOREACH (muv_0_1_0 IN cuv_0.MoviesAsActor |
        MERGE (m_0_1_0:Movie) SET m_0_1_0.Name=muv_0_1_0.Name, m_0_1_0.ReleaseDate=muv_0_1_0.ReleaseDate
        CREATE (c_0)-[:ACTED_IN]->(m_0_1_0)
        )
        """);
    }

    [Fact]
    public void Should_Create_Merge_Cypher_With_Simple_Single_Relation_On_First_Depth() {
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
        sut.Trim().Should().Be("""
        UNWIND $movies AS muv_0
        MERGE (m_0:Movie) SET m_0.Name=muv_0.Name, m_0.ReleaseDate=muv_0.ReleaseDate
        FOREACH (ignored IN CASE WHEN muv_0.Director IS NOT NULL THEN [1] ELSE [] END |
        MERGE (m_0_1_0:Person) SET m_0_1_0.FirstName=muv_0.Director.FirstName, m_0_1_0.LastName=muv_0.Director.LastName
        MERGE (m_0)<-[:DIRECTED]-(m_0_1_0)
        )
        """);
    }

    [Fact]
    public void Should_Create_Creation_Cypher_With_Simple_Single_Relation_On_First_Depth() {
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
        sut.Trim().Should().Be("""
        UNWIND $movies AS cuv_0
        CREATE (c_0:Movie) SET c_0.Name=cuv_0.Name, c_0.ReleaseDate=cuv_0.ReleaseDate
        FOREACH (ignored IN CASE WHEN cuv_0.Director IS NOT NULL THEN [1] ELSE [] END |
        MERGE (m_0_1_0:Person) SET m_0_1_0.FirstName=cuv_0.Director.FirstName, m_0_1_0.LastName=cuv_0.Director.LastName
        CREATE (c_0)<-[:DIRECTED]-(m_0_1_0)
        )
        """);
    }
}