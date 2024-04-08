using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using FluentAssertions;
using Neo4j.Berries.OGM.Tests.Mocks.Enums;
using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Tests.Models.Sets;

public class UpdateTests : TestBase
{
    private readonly Movie TestNode;
    private readonly Equipment TestEquipmentNode;
    public UpdateTests() : base(true)
    {
        TestNode = TestGraphContext.Movies.Match().FirstOrDefaultAsync().Result;
        TestEquipmentNode = TestGraphContext.Equipments.Match().FirstOrDefault();
    }

    [Fact]
    public async void Should_Only_Update_Year_Of_The_Movie()
    {
        TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .Update(x => x.Set(y => y.Name, "The Punisher"));

        var updatedNode = await TestGraphContext.Movies.Match(x => x.Where(y => y.Id, TestNode.Id)).FirstOrDefaultAsync();
        updatedNode.Should().NotBeNull();
        updatedNode.Id.Should().Be(TestNode.Id);
        updatedNode.Name.Should().Be("The Punisher");
    }
    [Fact]
    public async void Should_Update_And_Return_Value()
    {
        TestNode.ReleaseDate = new DateTime(1980, 01, 01);
        var result = await TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .UpdateAndReturnAsync(x => x.Set(TestNode));
        result.ElementAt(0).ReleaseDate.Should().Be(TestNode.ReleaseDate);
        result.ElementAt(0).Id.Should().Be(TestNode.Id);
    }

    [Fact]
    public async void Should_Create_New_Node_And_Connect_With_Existing()
    {
        var newPerson = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Fariborz",
            LastName = "Nowzari",
            Age = 31
        };
        TestGraphContext.People.Add(newPerson);
        await TestGraphContext.SaveChangesAsync();

        await TestGraphContext.Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .ConnectAsync(x => x.Actors, x => x.Where(y => y.Id, newPerson.Id));

        var count = await TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .WithRelation(x => x.Actors, x => x.Where(y => y.Id, newPerson.Id))
            .CountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async void Should_Throw_Exception_On_Disconnecting_Without_Relation()
    {
        var act = () => TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .DisconnectAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async void Should_Disconnect_Two_Nodes()
    {
        var person = await TestGraphContext
            .People
            .Match()
            .WithRelation(x => x.MoviesAsActor, x => x.Where(y => y.Id, TestNode.Id))
            .FirstOrDefaultAsync();

        await TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .WithRelation(x => x.Actors, x => x.Where(y => y.Id, person.Id))
            .DisconnectAsync();

        var count = await TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .WithRelation(x => x.Actors, x => x.Where(y => y.Id, person.Id))
            .CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async void Should_Remove_And_Return_Archived_Nodes()
    {
        var archivedNodes = await TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id))
            .ArchiveAsync();

        archivedNodes.Should().HaveCount(1);
        archivedNodes.Should().Contain(x => x.Id == TestNode.Id);

        var records = OpenSession(session =>
        {
            return session.Run(
                "MATCH(m:Movie WHERE m.Id=$p0) return m.ArchivedAt as archivedAt",
                new Dictionary<string, object> { { "p0", TestNode.Id.ToString() } })
                .Select(x =>
                {
                    var offset = DateTimeOffset.FromUnixTimeMilliseconds(x.Get<long>("archivedAt"));
                    return offset.UtcDateTime;
                })
                .ToList();
        });
        records.First().Should().BeBefore(DateTime.UtcNow);
        records.First().Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async void Test_Sequence_Of_Database_Actions_Against_Database_In_One_Transaction(bool withFailure)
    {
        var movieId = (await TestGraphContext
            .Movies
            .Match()
            .WithRelation(x => x.Actors)
            .FirstOrDefaultAsync()).Id;
        var sut = () => TestGraphContext.Database.BeginTransaction(async () =>
        {
            var movie = await TestGraphContext
                .Movies
                .Match(x => x.Where(y => y.Id, movieId))
                .WithRelation(x => x.Actors)
                .FirstOrDefaultAsync();

            await TestGraphContext
                .Movies
                .Match(x => x.Where(y => y.Id, movie.Id))
                .WithRelation(x => x.Actors)
                .DisconnectAsync();
            if (withFailure)
            {
                throw new Exception();
            }
            return movie;
        });
        if (!withFailure)
        {
            var movie = sut();
            var count = await TestGraphContext
                .People
                .Match()
                .WithRelation(x => x.MoviesAsActor, x => x.Where(y => y.Id, movie.Id))
                .CountAsync();
            count.Should().Be(0, "There should be no connection from any person to this movie anymore");
        }
        else
        {
            sut.Should().Throw<Exception>();
            var count = await TestGraphContext
                .People
                .Match()
                .WithRelation(x => x.MoviesAsActor, x => x.Where(y => y.Id, movieId))
                .CountAsync();
            count.Should().BeGreaterThan(0, "The transaction is failed, so the movie should not have it's relations disconnected");
        }
    }

    [Fact]
    public async void Should_Execute_Cyphers_Independently_From_Match_Phrase_And_Executed_Actions()
    {
        var query = TestGraphContext
            .People
            .Match()
            .WithRelation(x => x.MoviesAsActor, x => x.Where(y => y.Id, TestNode.Id));

        (await query.CountAsync()).Should().BeGreaterThan(0);

        var updateAct = () => query.UpdateAsync(x => x.Set(y => y.Age, 20));
        await updateAct.Should().NotThrowAsync();

        var connectId = Guid.NewGuid();
        var connectAct = () => query.ConnectAsync(x => x.Friends, x => x.Where(y => y.Id, connectId));
        await connectAct.Should().NotThrowAsync();

        var disconnectAct = () => query.DisconnectAsync();
        await disconnectAct.Should().NotThrowAsync();

        var archivedAct = () => query.ArchiveAsync();
        await archivedAct.Should().NotThrowAsync();

        (await query.CountAsync()).Should().Be(0, "Because it is disconnected");
    }

    [Fact]
    public async void Should_Update_Multiple_Properties()
    {
        var query = TestGraphContext
            .Movies
            .Match(x => x.Where(y => y.Id, TestNode.Id));

        var releaseDate = new DateTime(2004, 06, 10);
        await query.UpdateAsync(x => x
                .Set(y => y.Name, "The Punisher")
                .Set(y => y.ReleaseDate, releaseDate));

        var updatedNode = await TestGraphContext.Movies.Match(x => x.Where(y => y.Id, TestNode.Id)).FirstOrDefaultAsync();
        updatedNode.Should().NotBeNull();
        updatedNode.Id.Should().Be(TestNode.Id);
        updatedNode.Name.Should().Be("The Punisher");
        updatedNode.ReleaseDate.Should().Be(releaseDate);
    }
    [Fact]
    public void Should_Update_Nodes_Without_Config()
    {
        var result = TestGraphContext.Equipments.Match(x => x.Where(y => y.Id, TestEquipmentNode.Id)).UpdateAndReturn(x => x.Set(y => y.Type, EquipmentType.Light));

        result.ElementAt(0).Type.Should().Be(EquipmentType.Light);
    }

    [Fact]
    public void Should_Update_Movie_Anonymously()
    {
        var anonymous = TestGraphContext.Anonymous("Movie", builder =>
        {
            builder.HasRelation("Person", "ACTED_IN", RelationDirection.In);
        });
        anonymous.Match(x => x.Where("Id", TestNode.Id)).Update(x => x.Set("Name", "Anonymous"));
        var anonymousMovie = anonymous.Match(x => x.Where("Id", TestNode.Id)).FirstOrDefault<Dictionary<string, object>>();
        anonymousMovie["Name"].Should().Be("Anonymous");
    }
}