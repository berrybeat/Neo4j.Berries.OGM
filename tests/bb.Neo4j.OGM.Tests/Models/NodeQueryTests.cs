using berrybeat.Neo4j.OGM.Contexts;
using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Tests.Common;
using berrybeat.Neo4j.OGM.Tests.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;

namespace berrybeat.Neo4j.OGM.Tests.Models;

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
        query.Cypher.Should().Be("MATCH (l0:Person)\n");
    }
    [Fact]
    public void Should_Generate_Relations_Where_StartNode_And_EndNode_Are_Of_The_Same_Type()
    {
        var query = _graphContext
            .People
            .Match()
            .WithRelation(x => x.Friends);
        query.Matches.Should().HaveCount(2);
        query.Cypher.Should().Be("MATCH (l0:Person)\nMATCH (l0)-[r1:FRIENDS_WITH]->(l1:Person)\n");
    }
    [Fact]
    public void Should_Add_Relation_Match_With_Eloquent()
    {
        var query = _graphContext
            .People
            .Match()
            .WithRelation(x => x.Friends, eloquent => eloquent.Where(x => x.Id, Guid.NewGuid()));
        query.Matches.Should().HaveCount(2);
        query.Cypher.Should().Be("MATCH (l0:Person)\nMATCH (l0)-[r1:FRIENDS_WITH]->(l1:Person WHERE l1.Id = $qp_1_0)\n");
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
        query.Cypher.Should().Be("MATCH (l0:Person WHERE (l0.FirstName = $qp_0_0 AND l0.LastName = $qp_0_1))\nMATCH (l0)-[r1:FRIENDS_WITH]->(l1:Person WHERE (l1.Id = $qp_1_0 AND l1.Age > $qp_1_1))\n");
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
        query.Cypher.Should().Be("MATCH (l0:Movie)\nMATCH (l0)<-[r1:ACTED_IN]-(l1:Person WHERE l1.Id = $qp_1_0)\nMATCH (l0)<-[r2:DIRECTED]-(l2:Person WHERE l2.Age > $qp_2_0)\n");
    }
}