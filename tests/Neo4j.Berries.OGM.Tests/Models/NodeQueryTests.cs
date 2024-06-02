using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks;
using FluentAssertions;

namespace Neo4j.Berries.OGM.Tests.Models;

//This has to be set to Serial, otherwise will conflict with ObjectUtilsTests
[Collection("Serial")]
public class NodeQueryTests
{
    private readonly ApplicationGraphContext _graphContext;

    public NodeQueryTests()
    {
        _graphContext = new ApplicationGraphContext(new Neo4jOptions(ConfigurationsFactory.Config));
        _ = new Neo4jSingletonContext(GetType().Assembly);
    }



    [Fact]
    public void Should_Create_Cypher_Query_Without_Where_Clause()
    {
        var query = _graphContext
            .People
            .Match();
        query.Matches.Should().HaveCount(1);
        query.Cypher.Trim().Should().Be("MATCH (l0:Person)");
    }
    [Fact]
    public void Should_Generate_Relations_Where_StartNode_And_EndNode_Are_Of_The_Same_Type()
    {
        var query = _graphContext
            .People
            .Match()
            .WithRelation(x => x.Friends);
        query.Matches.Should().HaveCount(2);
        query.Cypher.Trim().Should().Be("""
        MATCH (l0:Person)
        MATCH (l0)-[r1:FRIENDS_WITH]->(l1:Person)
        """);
    }
    [Fact]
    public void Should_Add_Relation_Match_With_Eloquent()
    {
        var query = _graphContext
            .People
            .Match()
            .WithRelation(x => x.Friends, eloquent => eloquent.Where(x => x.Id, Guid.NewGuid()));
        query.Matches.Should().HaveCount(2);
        query.Cypher.Trim().Should().Be("""
        MATCH (l0:Person)
        MATCH (l0)-[r1:FRIENDS_WITH]->(l1:Person WHERE l1.Id = $qp_1_0)
        """);
    }
    [Fact]
    public void All_Matches_Must_Have_Eloquent()
    {
        var query = _graphContext
            .People
            .Match(x =>
            {
                x.Where(x => x.FirstName, "Farhad")
                .Where(x => x.LastName, "Nowzari");
                return x;
            })
            .WithRelation(x => x.Friends, eloquent => eloquent.Where(x => x.Id, Guid.NewGuid()).Where(x => x.Age, ComparisonOperator.GreaterThan, 18));
        query.Matches.Should().HaveCount(2);
        query.Cypher.Trim().Should().Be("""
        MATCH (l0:Person WHERE (l0.FirstName = $qp_0_0 AND l0.LastName = $qp_0_1))
        MATCH (l0)-[r1:FRIENDS_WITH]->(l1:Person WHERE (l1.Id = $qp_1_0 AND l1.Age > $qp_1_1))
        """);
    }
    [Fact]
    public void Should_Add_Multiple_Relation_Matches()
    {
        var query = _graphContext
            .Movies
            .Match()
            .WithRelation(x => x.Actors, x => x.Where(y => y.Id, Guid.NewGuid()))
            .WithRelation(x => x.Director, x => x.Where(y => y.Age, ComparisonOperator.GreaterThan, 18));
        query.Matches.Should().HaveCount(3);
        query.Cypher.Trim().Should().Be("""
        MATCH (l0:Movie)
        MATCH (l0)<-[r1:ACTED_IN]-(l1:Person WHERE l1.Id = $qp_1_0)
        MATCH (l0)<-[r2:DIRECTED]-(l2:Person WHERE l2.Age > $qp_2_0)
        """);
    }

    [Fact]
    public void Should_Generate_Cypher_For_Group_Matches()
    {
        var query = _graphContext
            .People
            .Match()
            .WithRelation(x => x.Resources);
        query.Matches.Should().HaveCount(2);
        query.Cypher.Trim().Should().Be("""
        MATCH (l0:Person)
        MATCH (l0)-[r1:USES]->(l1)
        """);
    }

    [Fact]
    public void Generated_Cypher_For_Group_Relations_Should_Be_Queryable()
    {
        var query = _graphContext
            .People
            .Match()
            .WithRelation(x => x.Resources, x => x.Where(y => y.Id, Guid.NewGuid()));
        query.Matches.Should().HaveCount(2);
        query.Cypher.Trim().Should().Be("""
        MATCH (l0:Person)
        MATCH (l0)-[r1:USES]->(l1 WHERE l1.Id = $qp_1_0)
        """);
    }
}