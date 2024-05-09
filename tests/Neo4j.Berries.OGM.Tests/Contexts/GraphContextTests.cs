using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Neo4j.Berries.OGM.Utils;
using FluentAssertions;
using Neo4j.Driver;


namespace Neo4j.Berries.OGM.Tests.Contexts;

public class GraphContextTests : TestBase
{
    [Fact]
    public void Init_AllNodeSets_OnCreatingNewInstance()
    {
        TestGraphContext.Movies.Should().NotBeNull();
        TestGraphContext.People.Should().NotBeNull();
        TestGraphContext.SilentMovies.Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_Creates_NewNodesInDatabase()
    {
        var matrix = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix"
        };
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann"
        };
        TestGraphContext.Movies.Add(matrix);
        TestGraphContext.People.Add(person);
        await TestGraphContext.SaveChangesAsync();

        var movieRecords = await TestGraphContext.Movies.Match(x => x.Where(y => y.Id, matrix.Id)).ToListAsync();
        movieRecords.Should().NotBeEmpty();
        movieRecords[0].Id.Should().Be(matrix.Id);
        movieRecords[0].Name.Should().Be(matrix.Name);


        var personRecords = await TestGraphContext
            .People
            .Match(x => x.Where(y => y.Id, person.Id))
            .ToListAsync();
        personRecords.Should().NotBeEmpty();
        personRecords[0].Id.Should().Be(person.Id);
        personRecords[0].FirstName.Should().Be(person.FirstName);
        personRecords[0].LastName.Should().Be(person.LastName);
        personRecords[0].BirthDate.Should().BeNull();
    }

    [Fact]
    //A node set is a valid one to save, when something is added to it.
    public void Should_Only_Save_Valid_NodeSets()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Age = 32,
        };
        TestGraphContext.People.Add(person);
        TestGraphContext.SaveChanges();

        var personRecord = TestGraphContext.People.Match(x => x.Where(y => y.Id, person.Id)).FirstOrDefault();
        personRecord.Should().NotBeNull();
        personRecord.Id.Should().Be(person.Id);
        personRecord.FirstName.Should().Be(person.FirstName);
        personRecord.LastName.Should().Be(person.LastName);
        personRecord.Age.Should().Be(person.Age);

    }

    [Fact]
    public async void SaveChanges_Creates_A_Collection()
    {
        var movies = new List<Movie>() {
            new() {
                Id = Guid.NewGuid(),
                Name = "Matrix"
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Punisher"
            }
        };

        TestGraphContext.Movies.AddRange(movies);
        await TestGraphContext.SaveChangesAsync();

        var movieRecords = await TestGraphContext
            .Movies
            .Match(x => x.WhereIsIn(y => y.Id, movies.Select(z => z.Id)))
            .ToListAsync();
        movieRecords.Should().HaveCount(movies.Count);
    }

    [Fact]
    public async void Should_Create_Node_With_Single_Relations()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            Director = new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Lana",
                LastName = "Wachowski",
                BirthDate = new DateTime(1999, 01, 01)
            }
        };
        TestGraphContext.Movies.Add(movie);
        await TestGraphContext.SaveChangesAsync();
        var records = OpenSession(session =>
        {
            return session.Run(
                "match(m:Movie)<-[:DIRECTED]-(p:Person) where m.Id=$id return m,p",
                new { id = movie.Id.ToString() }
            )
            .Select(x => (movie: x.Convert<Movie>("m"), person: x.Convert<Person>("p")))
            .ToList();
        });
        records[0].movie.Id.Should().Be(movie.Id);
        records[0].movie.Name.Should().Be(movie.Name);
        records[0].person.Id.Should().Be(movie.Director.Id);
        records[0].person.FirstName.Should().BeNullOrEmpty("On merge, only Id is included in the properties map.");
        records[0].person.LastName.Should().BeNullOrEmpty("On merge, only Id is included in the properties map.");
        records[0].person.BirthDate.Should().BeNull("On merge, only Id is included in the properties map.");
    }

    [Fact]
    public async void Should_Create_Node_With_Multiple_Relations()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            Actors = new List<Person>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Keanu",
                    LastName = "Reeves"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Laurence",
                    LastName = "Fishburne"
                }
            }
        };
        TestGraphContext.Movies.Add(movie);
        await TestGraphContext.SaveChangesAsync();
        var records = OpenSession(session =>
        {
            return session.Run(
                "match(m:Movie)<-[:ACTED_IN]-(p:Person) where m.Id=$id return m,p",
                new { id = movie.Id.ToString() }
            )
            .Select(x => (movie: x.Convert<Movie>("m"), person: x.Convert<Person>("p")))
            .ToList();
        });
        records[0].movie.Id.Should().Be(movie.Id);
        records[0].movie.Name.Should().Be(movie.Name);

        var actor1 = records.First(x => x.person.Id == movie.Actors[0].Id).person;
        var actor2 = records.First(x => x.person.Id == movie.Actors[1].Id).person;

        actor1.FirstName.Should().BeNullOrEmpty("On merge, only Id is included in the properties map. (by config)");
        actor1.LastName.Should().BeNullOrEmpty("On merge, only Id is included in the properties map. (by config)");

        actor2.Id.Should().Be(movie.Actors[1].Id);
        actor2.FirstName.Should().BeNullOrEmpty("On merge, only Id is included in the properties map. (by config)");
    }

    [Fact]
    public async void Should_Be_Able_To_Call_AddRange_Multiple_Times()
    {
        var moviesCollection1 = new List<Movie>() {
            new() {
                Id = Guid.NewGuid(),
                Name = "Matrix"
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Punisher"
            }
        };
        var moviesCollection2 = new List<Movie>() {
            new() {
                Id = Guid.NewGuid(),
                Name = "Avengers"
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Thor"
            }
        };
        TestGraphContext.Movies.AddRange(moviesCollection1);
        TestGraphContext.Movies.AddRange(moviesCollection2);
        await TestGraphContext.SaveChangesAsync();
    }

    [Fact]
    public async void Should_Save_Multiple_Times_With_Transaction()
    {
        var movie1 = FakeMovies.GetMovie(1, 1).First();
        var movie2 = FakeMovies.GetMovie(1, 1).First();
        TestGraphContext.Database.BeginTransaction(async () =>
        {
            TestGraphContext.Movies.Add(movie1);
            await TestGraphContext.SaveChangesAsync();
            TestGraphContext.Movies.Add(movie2);
            await TestGraphContext.SaveChangesAsync();
        });
        (await TestGraphContext.Movies
            .Match(x => x.WhereIsIn(y => y.Id, [movie1.Id, movie2.Id]))
            .CountAsync())
            .Should()
            .Be(2);
    }
    [Fact]
    public async void Should_Roll_Back_If_Transaction_Body_Failed()
    {
        var movie1 = FakeMovies.GetMovie(1, 1).First();
        var movie2 = FakeMovies.GetMovie(1, 1).First();
        var act = () => TestGraphContext.Database.BeginTransaction(async () =>
        {
            TestGraphContext.Movies.Add(movie1);
            await TestGraphContext.SaveChangesAsync();
            throw new Exception("test");
#pragma warning disable CS0162
            TestGraphContext.Movies.Add(movie2);
            await TestGraphContext.SaveChangesAsync();
        });
        act.Should().Throw<Exception>();
        (await TestGraphContext.Movies
            .Match(x => x.WhereIsIn(y => y.Id, [movie1.Id, movie2.Id]))
            .CountAsync())
            .Should()
            .Be(0);
    }
    [Fact]
    public async void Should_Create_Node_With_Children_Based_On_Merge_Config_With_More_Than_One_Props()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Farhad",
            LastName = "Nowzari",
            Age = 32,
            BirthDate = new DateTime(1991, 01, 10),
            Friends = new List<Person> {
                new Person {
                    Age = 50,
                    FirstName = "Bruce",
                    LastName = "Wayne",
                    Id = Guid.NewGuid()
                }
            }
        };
        TestGraphContext.People.Add(person);
        await TestGraphContext.SaveChangesAsync();
        var farhad = await TestGraphContext.People.Match(x => x.Where(y => y.Id, person.Id)).FirstOrDefaultAsync();
        farhad.Should().NotBeNull();
        farhad.FirstName.Should().Be(person.FirstName);
        farhad.LastName.Should().Be(person.LastName);
        farhad.Age.Should().Be(person.Age);
        farhad.BirthDate.Should().NotBeNull();
        farhad.BirthDate.Should().Be(person.BirthDate);

        var friend = await TestGraphContext.People.Match(x => x.Where(y => y.Id, person.Friends.First().Id)).FirstOrDefaultAsync();
        friend.Age.Should().Be(person.Friends[0].Age);
        friend.FirstName.Should().BeNullOrEmpty();
        friend.LastName.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Should_Create_Anonymous_Node_With_Group_Relations()
    {
        var id = Guid.NewGuid();
        TestGraphContext.Anonymous("Person")
            .Merge(new() {
                { "Id", id },
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
            });
        TestGraphContext.SaveChanges();
        var records = TestGraphContext
            .Database
            .Session
            .Run("MATCH(person:Person)-[:USES]->(r:Room) WHERE person.Id=$id return distinct person", new { id = id.ToString() })
            .ToList();
        records.Should().NotBeEmpty();
        records.Should().HaveCount(1);

        records = [.. TestGraphContext
            .Database
            .Session
            .Run("MATCH(person:Person)-[:USES]->(c:Car) WHERE person.Id=$id return distinct person", new { id = id.ToString() })];
        records.Should().NotBeEmpty();
        records.Should().HaveCount(1);
        
    }
}