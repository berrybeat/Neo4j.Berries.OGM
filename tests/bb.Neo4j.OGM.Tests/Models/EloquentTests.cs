using System.Runtime.CompilerServices;
using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Models;
using berrybeat.Neo4j.OGM.Tests.Mocks;
using berrybeat.Neo4j.OGM.Tests.Mocks.Models;
using FluentAssertions;

namespace berrybeat.Neo4j.OGM.Tests.Models;

public class EloquentTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(ComparisonOperator.Equals)]
    [InlineData(ComparisonOperator.NotEquals)]
    [InlineData(ComparisonOperator.GreaterThan)]
    [InlineData(ComparisonOperator.GreaterThanOrEquals)]
    [InlineData(ComparisonOperator.LessThan)]
    [InlineData(ComparisonOperator.LessThanOrEquals)]
    public void Should_Generate_Simplest_Where_Clause(ComparisonOperator? comparisonOperator = null)
    {
        var query = new Eloquent<Person>(0);
        if (comparisonOperator.HasValue)
        {
            query.Where(x => x.FirstName, comparisonOperator.Value, "Farhad");
        }
        else
        {
            query.Where(x => x.FirstName, "Farhad");
        }
        var cypher = query.ToCypher("p");
        if (comparisonOperator.HasValue)
        {
            cypher.Should().Be($"p.FirstName {OperatorMaps.ComparisonOperatorMap[comparisonOperator.Value]} $qp_0_0");
        }
        else
        {
            cypher.Should().Be("p.FirstName = $qp_0_0");
        }
        query.QueryParameters["qp_0_0"].Should().Be("Farhad");
    }
    [Fact]
    public void Should_Generate_IS_NULL_Cypher_Query()
    {
        var query = new Eloquent<Person>(0);
        query.WhereIsNull(x => x.Age);
        query.ToCypher("p").Should().Be("p.Age IS NULL");
        query.QueryParameters.Should().BeEmpty();
    }
    [Fact]
    public void Should_Generate_IS_NOT_NULL_Cypher_Query()
    {
        var query = new Eloquent<Person>(0);
        query.WhereIsNotNull(x => x.Age);
        query.ToCypher("p").Should().Be("p.Age IS NOT NULL");
        query.QueryParameters.Should().BeEmpty();
    }
    [Fact]
    public void Should_Generate_IS_IN_Cypher_Query()
    {
        int[] ages = [30, 25, 10];
        var query = new Eloquent<Person>(0);
        query.WhereIsIn(x => x.Age, ages);
        query.ToCypher("p").Should().Be("p.Age IN $qp_0_0");
        query.QueryParameters["qp_0_0"].Should().BeEquivalentTo(ages);
    }
    [Fact]
    public void Should_Generate_IS_NOT_IN_Cypher_Query()
    {
        int[] ages = [30, 25, 10];
        var query = new Eloquent<Person>(0);
        query.WhereIsNotIn(x => x.Age, ages);
        query.ToCypher("p").Should().Be("NOT p.Age IN $qp_0_0");
        query.QueryParameters["qp_0_0"].Should().BeEquivalentTo(ages);
    }
    [Fact]
    public void Should_Generate_Cypher_Query_With_Multiple_ORs()
    {
        var query = new Eloquent<Person>(0);
        query
            .OR
            .Where(x => x.Id, Guid.NewGuid())
            .Where(x => x.FirstName, "Farhad")
            .Where(x => x.LastName, "Nowzari");
        query.ToCypher("p").Should().Be("(p.Id = $qp_0_0 OR p.FirstName = $qp_0_1 OR p.LastName = $qp_0_2)");
        query.QueryParameters["qp_0_0"].Should().BeOfType<string>(); //neo4j doesn't understand Guid
        query.QueryParameters["qp_0_1"].Should().Be("Farhad");
        query.QueryParameters["qp_0_2"].Should().Be("Nowzari");
    }
    [Fact]
    public void Should_Generate_Cypher_Query_With_XOR()
    {
        var query = new Eloquent<Person>(0);
        query
            .XOR
            .Where(x => x.Id, ComparisonOperator.NotEquals, Guid.NewGuid())
            .Where(x => x.FirstName, "Farhad");
        query.ToCypher("p").Should().Be("(p.Id <> $qp_0_0 XOR p.FirstName = $qp_0_1)");
        query.QueryParameters["qp_0_0"].Should().BeOfType<string>();
        query.QueryParameters["qp_0_1"].Should().Be("Farhad");
    }
    [Fact]
    public void Should_Generate_Mixed_Cypher_Query()
    {
        var query = new Eloquent<Person>(0);
        query
            .Where(x => x.Id, Guid.NewGuid())
            .OR
            .Where(x => x.FirstName, "Farhad")
            .XOR
            .Where(x => x.LastName, "Nowzari")
            .Where(x => x.Age, ComparisonOperator.GreaterThan, 30);
        query.ToCypher("p").Should().Be("p.Id = $qp_0_0 OR p.FirstName = $qp_0_1 XOR (p.LastName = $qp_0_2 XOR p.Age > $qp_0_3)");
        query.QueryParameters["qp_0_0"].Should().BeOfType<string>();
        query.QueryParameters["qp_0_1"].Should().Be("Farhad");
        query.QueryParameters["qp_0_2"].Should().Be("Nowzari");
    }

    [Fact]
    public void IS_NULL_Should_Be_Ignored_As_A_Query_Parameter()
    {
        var query = new Eloquent<Person>(0);
        query
            .WhereIsNull(x => x.LastName)
            .Where(x => x.Age, ComparisonOperator.GreaterThanOrEquals, 30);
        query.QueryParameters["qp_0_0"].Should().Be(30);
        query.QueryParameters.Should().HaveCount(1);
    }

    [Fact]
    public void IS_IN_Should_Convert_Guid_To_String()
    {
        var query = new Eloquent<Person>(0);
        query.WhereIsIn(x => x.Id, [Guid.NewGuid(), Guid.NewGuid()]);
        query.QueryParameters["qp_0_0"].Should().BeOfType<string[]>();
    }

    [Fact]
    public void IS_NOT_IN_Should_Convert_Guid_To_String()
    {
        var query = new Eloquent<Person>(0);
        query.WhereIsNotIn(x => x.Id, [Guid.NewGuid(), Guid.NewGuid()]);
        query.QueryParameters["qp_0_0"].Should().BeOfType<string[]>();
    }

    [Fact]
    public void Should_Query_By_A_Nullable_Query_Variable_Which_Has_Value()
    {
        Guid? nullableId = Guid.NewGuid();
        var query = new Eloquent<Person>(0);
        query.Where(x => x.Id, nullableId);

        query.QueryParameters["qp_0_0"].Should().Be(nullableId.ToString());
    }
    [Fact]
    public void Should_Query_By_A_Nullable_Query_Variable_Which_Is_Null()
    {
        Guid? nullableId = null;
        var query = new Eloquent<Person>(0);
        query.Where(x => x.Id, nullableId);

        query.QueryParameters.Should().BeEmpty();
        query.ToCypher("p").Should().Be("p.Id IS NULL");
    }
}