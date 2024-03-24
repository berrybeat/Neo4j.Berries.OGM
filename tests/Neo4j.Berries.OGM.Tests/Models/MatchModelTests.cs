using System.Text;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Match;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using FluentAssertions;

namespace Neo4j.Berries.OGM.Tests.Models;

public class MatchModelTests
{
    public StringBuilder CypherBuilder { get; }

    public MatchModelTests()
    {
        CypherBuilder = new StringBuilder();
    }
    [Fact]
    public void Should_Create_Match_Without_Eloquent()
    {
        var sut = new MatchModel<Person>(null, 0);
        sut.ToCypher(CypherBuilder);
        CypherBuilder.ToString().Should().Be("MATCH (l0:Person)\n");
    }
    [Fact]
    public void Should_Create_Match_With_Eloquent()
    {
        var eloquent = new Eloquent<Person>(0);
        eloquent.Where(x => x.Id, Guid.NewGuid());
        var sut = new MatchModel<Person>(eloquent, 0);
        sut.ToCypher(CypherBuilder);
        CypherBuilder.ToString().Should().Be("MATCH (l0:Person WHERE l0.Id = $qp_0_0)\n");
    }
    [Fact]
    public void Should_Create_Match_OutGoing_Relation_Without_Eloquent()
    {
        var firstMatch = new MatchModel<Person>(null, 0);
        var relationConfig = new RelationConfiguration<Person, Movie>("ACTED_IN", RelationDirection.Out);
        var sut = new MatchRelationModel<Movie>(firstMatch, relationConfig, null, 1);
        sut.ToCypher(CypherBuilder);
        CypherBuilder.ToString().Should().Be("MATCH (l0)-[r1:ACTED_IN]->(l1:Movie)\n");
    }
    [Fact]
    public void Should_Create_Match_OutGoing_Relation_With_Eloquent()
    {
        var firstMatch = new MatchModel<Person>(null, 0);
        var eloquent = new Eloquent<Movie>(1);
        eloquent.Where(x => x.Id, Guid.NewGuid());
        var relationConfig = new RelationConfiguration<Person, Movie>("ACTED_IN", RelationDirection.Out);
        var sut = new MatchRelationModel<Movie>(firstMatch, relationConfig, eloquent, 1);
        sut.ToCypher(CypherBuilder);
        CypherBuilder.ToString().Should().Be("MATCH (l0)-[r1:ACTED_IN]->(l1:Movie WHERE l1.Id = $qp_1_0)\n");
    }
    [Fact]
    public void Should_Create_Match_InComing_Relation_Without_Eloquent()
    {
        var firstMatch = new MatchModel<Movie>(null, 0);
        var relationConfig = new RelationConfiguration<Movie, Person>("ACTED_IN", RelationDirection.In);
        var sut = new MatchRelationModel<Person>(firstMatch, relationConfig, null, 1);
        sut.ToCypher(CypherBuilder);
        CypherBuilder.ToString().Should().Be("MATCH (l0)<-[r1:ACTED_IN]-(l1:Person)\n");
    }
    [Fact]
    public void Should_Create_Match_InComing_Relation_With_Eloquent()
    {
        var firstMatch = new MatchModel<Movie>(null, 0);
        var eloquent = new Eloquent<Person>(1);
        eloquent.Where(x => x.Id, Guid.NewGuid());
        var relationConfig = new RelationConfiguration<Person, Person>("ACTED_IN", RelationDirection.In);
        var sut = new MatchRelationModel<Person>(firstMatch, relationConfig, eloquent, 1);
        sut.ToCypher(CypherBuilder);
        CypherBuilder.ToString().Should().Be("MATCH (l0)<-[r1:ACTED_IN]-(l1:Person WHERE l1.Id = $qp_1_0)\n");
    }
}