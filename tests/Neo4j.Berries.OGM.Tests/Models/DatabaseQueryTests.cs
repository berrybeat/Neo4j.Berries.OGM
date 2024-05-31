using Neo4j.Berries.OGM.Tests.Common;
using FluentAssertions;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Tests.Models;


public class DatabaseQueryTests() : TestBase(true)
{
    [Fact]
    public async void Should_Count_All_Movies()
    {
        var query = TestGraphContext
            .Movies
            .Match();
        (await query.CountAsync()).Should().Be(10);
        query.Count().Should().Be(10);
    }

    [Fact]
    public async void Should_Count_All_Movies_Which_Have_Actors()
    {
        var count = await TestGraphContext
            .Movies
            .Match()
            .WithRelation(x => x.Actors)
            .CountAsync();
        count.Should().Be(10);
    }
    [Fact]
    public async void Should_Count_All_Movies_Which_Only_Have_Director()
    {
        var count = await TestGraphContext
            .Movies
            .Match()
            .WithRelation(x => x.Director)
            .CountAsync();
        count.Should().Be(5);
    }
    [Fact]
    public async void Should_Return_0_When_The_Condition_Is_Not_Met()
    {
        var count = await TestGraphContext
        .Movies
        .Match()
        .WithRelation(x => x.Actors, x => x.Where(y => y.Id, Guid.NewGuid()))
        .CountAsync();
        count.Should().Be(0);
    }
    [Fact]
    public async void Should_Return_First_Director()
    {
        var query = TestGraphContext
            .People
            .Match()
            .WithRelation(x => x.MoviesAsDirector);

        (await query.FirstOrDefaultAsync()).Should().NotBeNull();
        query.FirstOrDefault().Id.Should().NotBe(Guid.Empty);
    }
    [Fact]
    public async void Should_Return_Null_When_Condition_Does_NOT_Match()
    {
        var director = await TestGraphContext
            .People
            .Match(x => x.Where(y => y.Id, Guid.NewGuid()))
            .WithRelation(x => x.MoviesAsDirector)
            .FirstOrDefaultAsync();
        director.Should().BeNull();
    }
    [Fact]
    public async void Get_List_Of_Movies()
    {
        var query = TestGraphContext
            .Movies
            .Match();
        var movies = await query.ToListAsync();
        movies.Should().HaveCount(10);
        query.ToList().Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void Should_Execute_Any_On_Match(bool result)
    {
        var query = TestGraphContext
            .Movies
            .Match();

        if (result)
        {
            query = query.WithRelation(x => x.Actors);
        }
        else
        {
            query = query.WithRelation(x => x.Actors, x => x.Where(y => y.Id, Guid.NewGuid()));
        }

        (await query.AnyAsync()).Should().Be(result);
    }

    [Fact]
    public void Locking_Should_Throw_InvalidOperationException_If_No_Transaction_Is_Open()
    {
        var act = () => TestGraphContext
            .People
            .Match()
            .Lock();

        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Lock/Unlock should only be used within an explicitly opened transaction!");
    }

    [Fact]
    public void Lock_Should_Add_Lock_Flag_On_Queries_Nodes()
    {
        TestGraphContext.Database.BeginTransaction(() => {
            TestGraphContext
                .People
                .Match()
                .Lock();

            var neo4jDatabase = ConfigurationsFactory.Config["Neo4j:Database"];
            var _session = TestGraphContext.Database
                .Driver
                .Session(config => config.WithDatabase(neo4jDatabase));
            var result = _session
                .Run("SHOW TRANSACTION YIELD activeLockCount")
                .ToList()
                .Select(x => x["activeLockCount"]);

            result.Should().Contain(x => int.Parse(x.ToString()) > 0);

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async void Should_Multiple_Query_Actions_Should_Use_One_Match_Query()
    {
        var query = TestGraphContext
            .Movies
            .Match()
            .WithRelation(x => x.Actors);
        (await query.CountAsync()).Should().BeGreaterThan(0);
        (await query.FirstOrDefaultAsync()).Should().NotBeNull();
        (await query.ToListAsync()).Should().NotBeEmpty();
        (await query.AnyAsync()).Should().BeTrue();
        //TODO Add more assertions based on new methods
    }

}