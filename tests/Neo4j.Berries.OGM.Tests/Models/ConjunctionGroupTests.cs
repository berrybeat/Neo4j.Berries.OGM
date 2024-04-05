using Neo4j.Berries.OGM.Models;
using FluentAssertions;
using Neo4j.Berries.OGM.Models.Queries;

namespace Neo4j.Berries.OGM.Tests.Models;

public class ConjunctionGroupTests
{
    [Fact]
    public void Should_Create_A_Condition_String_For_Given_Conjunction()
    {
        var sut = new ConjunctionGroup
        {
            Conjunction = "AND",
            Members = new List<ConjunctionGroupMember> {
                new(Operand1: "name", Operator: "=", Operand2: "$name"),
                new(Operand1: "age", Operator: ">", Operand2:"$age" )
            }
        };
        var result = sut.ToString("person");
        result.Should().Be("(person.name = $name AND person.age > $age)");
    }
    [Fact]
    public void Should_Overwrite_The_Operator_S_Format()
    {
        var sut = new ConjunctionGroup
        {
            Conjunction = "AND",
            Members = new List<ConjunctionGroupMember> {
                new(Operand1: "name", Operator: "NOT {0} IN {1}", Operand2: "$names", true),
                new(Operand1: "age", Operator: ">", Operand2:"$age" )
            }
        };
        var result = sut.ToString("person");
        result.Should().Be("(NOT person.name IN $names AND person.age > $age)");
    }

    [Fact]
    public void When_There_Is_Only_One_Member_No_Parenthesis_Is_Needed()
    {
        var sut = new ConjunctionGroup
        {
            Conjunction = "AND",
            Members = new List<ConjunctionGroupMember> {
                new(Operand1: "age", Operator: ">", Operand2:"$age" )
            }
        };
        var result = sut.ToString("person");
        result.Should().Be("person.age > $age");
    }
}