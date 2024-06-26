using FluentAssertions;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Tests.Utils;

//This has to be set to serial, otherwise will conflict with NodeQueryTests
[Collection("Serial")]
public class ObjectUtilsTests
{
    #region ToDictionary
    [Fact]
    public void Should_Convert_Object_To_Dictionary()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
        };
        var result = movie.ToDictionary([]);
        result.Should().HaveCount(2);
        result["Id"].Should().Be(movie.Id.ToString());
        result["Name"].Should().Be(movie.Name);
    }

    [Fact]
    public void Should_Add_Single_Objects_To_Dictionary()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            ReleaseDate = DateTime.Now,
            Director = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Lana",
                LastName = "Wachowski",
            }
        };
        Dictionary<string, NodeConfiguration> config = new() {
            { "Movie", new () }
        };
        config["Movie"].Relations.TryAdd("Director", new RelationConfiguration<Person, Movie>("Director", RelationDirection.Out));
        var result = movie.ToDictionary(config);
        result["ReleaseDate"].Should().Be(movie.ReleaseDate);
        var directorDictionary = result["Director"] as Dictionary<string, object>;
        directorDictionary["Id"].Should().Be(movie.Director.Id.ToString());
        directorDictionary["FirstName"].Should().Be(movie.Director.FirstName);
        directorDictionary["LastName"].Should().Be(movie.Director.LastName);
    }
    [Fact]
    public void Should_Add_Lists_To_Dictionary()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            ReleaseDate = DateTime.Now,
            Actors = [
                new Person {
                    Id = Guid.NewGuid(),
                    FirstName = "Keanu",
                    LastName = "Reeves",
                },
                new Person {
                    Id = Guid.NewGuid(),
                    FirstName = "Carrie-Anne",
                    LastName = "Moss",
                }
            ]
        };
        Dictionary<string, NodeConfiguration> config = new() {
            { "Movie", new () }
        };
        config["Movie"]
            .Relations
            .TryAdd(
                "Actors",
                new RelationConfiguration<Person, Movie>("ACTED_IN", RelationDirection.In)
            );
        var result = movie.ToDictionary(config);
        var actors = result["Actors"] as IEnumerable<Dictionary<string, object>>;
        actors.Should().HaveCount(2);
        actors.First()["Id"].Should().Be(movie.Actors.First().Id.ToString());
        actors.First()["FirstName"].Should().Be(movie.Actors.First().FirstName);
        actors.First()["LastName"].Should().Be(movie.Actors.First().LastName);

        actors.Last()["Id"].Should().Be(movie.Actors.Last().Id.ToString());
        actors.Last()["FirstName"].Should().Be(movie.Actors.Last().FirstName);
        actors.Last()["LastName"].Should().Be(movie.Actors.Last().LastName);
    }
    [Fact]
    public void Should_Only_Iterate_One_Time()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            ReleaseDate = DateTime.Now,
            Director = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Lana",
                LastName = "Wachowski",
                MoviesAsActor = [
                    new Movie {
                        Id = Guid.NewGuid(),
                        Name = "Matrix",
                    },
                    new Movie {
                        Id = Guid.NewGuid(),
                        Name = "Matrix Reloaded",
                    }
                ]
            }
        };
        Dictionary<string, NodeConfiguration> config = new() {
            { "Movie", new () },
            { "Person", new () }
        };
        config["Movie"]
            .Relations
            .TryAdd(
                "Director",
                new RelationConfiguration<Person, Movie>("DIRECTED_BY", RelationDirection.In)
            );
        config["Person"]
            .Relations
            .TryAdd(
                "MoviesAsActor",
                new RelationConfiguration<Person, Movie>("ACTED_IN", RelationDirection.Out)
            );
        var result = movie.ToDictionary(config);
        var director = result["Director"] as Dictionary<string, object>;
        director["Id"].Should().Be(movie.Director.Id.ToString());
        director["FirstName"].Should().Be(movie.Director.FirstName);
        director["LastName"].Should().Be(movie.Director.LastName);
        var moviesAsActor = director["MoviesAsActor"];
        moviesAsActor.Should().BeNull();
    }
    [Fact]
    public void Should_Exclude_Properties()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            ReleaseDate = DateTime.Now,
            Director = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Lana",
                LastName = "Wachowski",
            }
        };
        Dictionary<string, NodeConfiguration> config = new() {
            { "Movie", new () }
        };
        config["Movie"].ExcludedProperties.Add("ReleaseDate");
        var result = movie.ToDictionary(config);
        result["Id"].Should().Be(movie.Id.ToString());
        result["Name"].Should().Be(movie.Name);
        result.Should().ContainKey("Director");
        result.Should().NotContainKey("ReleaseDate");
    }
    [Fact]
    public void Should_Only_Include_Merge_Properties()
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Name = "Matrix",
            ReleaseDate = DateTime.Now,
            Director = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Lana",
                LastName = "Wachowski",
            }
        };
        Dictionary<string, NodeConfiguration> config = new() {
            { "Movie", new () }
        };
        var relationConfig = new RelationConfiguration<Person, Movie>("DIRECTED_BY", RelationDirection.In);
        relationConfig.OnMerge().Include(x => x.Id);
        config["Movie"].Relations.TryAdd(
            "Director",
            relationConfig
        );
        var result = movie.ToDictionary(config);
        var director = result["Director"] as Dictionary<string, object>;
        director.Should().ContainKey("Id");
        director.Should().OnlyContain(x => x.Key == "Id");
    }
    [Fact]
    public void Should_Group_An_Interface_List_By_Different_Types_In_The_List()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Keanu",
            LastName = "Reeves",
            Resources = [
                new Room
                {
                    Number = "1234",
                },
                new Car
                {
                    LicensePlate = "ABC123",
                    Brand = "Toyota",
                    Model = "Corolla",
                }
            ]
        };
        _ = new Neo4jSingletonContext(GetType().Assembly);
        var result = person.ToDictionary(Neo4jSingletonContext.Configs);

        result.Should().ContainKey("Resources");
        result["Resources"].Should().BeOfType<Dictionary<string, List<Dictionary<string, object>>>>();
        var resources = result["Resources"] as Dictionary<string, List<Dictionary<string, object>>>;
        resources.Should().HaveCount(2);
        resources.Should().ContainKey("Car");
        resources.Should().ContainKey("Room");
        resources["Room"].Should().HaveCount(1);
        resources["Car"].Should().HaveCount(1);

        resources["Room"].First().Should().ContainKey("Number");
        resources["Room"].First()["Number"].Should().Be("1234");

        resources["Car"].First().Should().ContainKey("LicensePlate");
        resources["Car"].First().Should().ContainKey("Brand");
        resources["Car"].First().Should().ContainKey("Model");

        resources["Car"].First()["LicensePlate"].Should().Be("ABC123");
        resources["Car"].First()["Brand"].Should().Be("Toyota");
        resources["Car"].First()["Model"].Should().Be("Corolla");
    }

    [Fact]
    public void Should_Convert_All_Properties_To_CamelCase()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Keanu",
            LastName = "Reeves",
            Address = new Address
            {
                AddressLine = "1234",
                City = "Los Angeles",
                Country = "USA",
                PostalCode = "12345",
            },
            MoviesAsActor = [
                new Movie
                {
                    Id = Guid.NewGuid(),
                    Name = "Matrix",
                },
                new Movie
                {
                    Id = Guid.NewGuid(),
                    Name = "Matrix Reloaded",
                }
            ],
            Resources = [
                new Room
                {
                    Number = "1234",
                },
                new Car
                {
                    LicensePlate = "ABC123",
                    Brand = "Toyota",
                    Model = "Corolla",
                }
            ]
        };
        Dictionary<string, NodeConfiguration> config = new() {
            { "Movie", new () },
            { "Person", new () }
        };
        config["Person"]
            .Relations
            .TryAdd(
                "MoviesAsActor",
                new RelationConfiguration<Person, Movie>("ACTED_IN", RelationDirection.Out)
            );
        config["Person"]
            .Relations
            .TryAdd(
                "Address",
                new RelationConfiguration<Person, Address>("LIVES_IN", RelationDirection.Out)
            );
        config["Person"]
            .Relations
            .TryAdd(
                "Resources",
                new RelationConfiguration<Person, IResource>("HAS", RelationDirection.Out)
            );
        var result = person.ToDictionary(config, (input) => $"{char.ToLower(input[0])}{input[1..]}");
        result.Should().ContainKey("id");
        result.Should().ContainKey("firstName");
        result.Should().ContainKey("lastName");
        result.Should().ContainKey("Address");
        result.Should().ContainKey("MoviesAsActor");
        result.Should().ContainKey("Resources");

        var address = result["Address"] as Dictionary<string, object>;
        address.Should().ContainKey("addressLine");
        address.Should().ContainKey("city");
        address.Should().ContainKey("country");
        address.Should().ContainKey("postalCode");

        var moviesAsActor = result["MoviesAsActor"] as IEnumerable<Dictionary<string, object>>;
        moviesAsActor.Should().HaveCount(2);
        moviesAsActor.First().Should().ContainKey("id");
        moviesAsActor.First().Should().ContainKey("name");
        moviesAsActor.Last().Should().ContainKey("id");
        moviesAsActor.Last().Should().ContainKey("name");

        var resources = result["Resources"] as Dictionary<string, List<Dictionary<string, object>>>;
        resources.Should().HaveCount(2);
        resources.Should().ContainKey("Car");
        resources.Should().ContainKey("Room");
        resources["Room"].Should().HaveCount(1);
        resources["Car"].Should().HaveCount(1);

        var room = resources["Room"].First();
        room.Should().ContainKey("number");

        var car = resources["Car"].First();
        car.Should().ContainKey("licensePlate");
        car.Should().ContainKey("brand");
        car.Should().ContainKey("model");
    }
    #endregion

    #region NormalizeValuesForNeo4j
    [Fact]
    public void Should_Convert_Guid_To_String()
    {
        var movie = new Dictionary<string, object> {
            { "Id", Guid.NewGuid() },
            { "Name", "The Matrix" }
        };
        var result = movie.NormalizeValuesForNeo4j();
        result["Id"].Should().BeOfType(typeof(string));
        result["Id"].Should().Be(movie["Id"].ToString());
    }
    [Fact]
    public void Should_Not_Include_Null_Values_For_Props_In_Relations()
    {
        var movie = new Dictionary<string, object> {
            { "Id", Guid.NewGuid() },
            { "Name", "The Matrix" },
            { "ReleaseDate", null },
            { "Director", new Dictionary<string, object> {
                { "Name", "Lana Wachowski" },
                { "Born", 1965 },
                { "BirthDate", null }
            } }
        };
        var result = movie.NormalizeValuesForNeo4j();
        result["ReleaseDate"].Should().BeNull();
        var director = result["Director"] as Dictionary<string, object>;
        director.Should().NotContainKey("BirthDate");
    }
    [Fact]
    public void Should_Normalize_Arrays_Of_Reference_Types() {
        var movie = new Dictionary<string, object> {
            { "Id", Guid.NewGuid() },
            { "Name", "The Matrix" },
            { "ReleaseDate", null },
            { "Actors", new List<Dictionary<string, object>> {
                new() {
                    { "Name", "Keanu Reeves" }
                },
                new() {
                    { "Name", "Carrie-Anne Moss" }
                }
            }}
        };
        var result = movie.NormalizeValuesForNeo4j();
        var actors = result["Actors"] as IEnumerable<Dictionary<string, object>>;
        actors.Should().HaveCount(2);
        actors.First()["Name"].Should().Be("Keanu Reeves");
        actors.Last()["Name"].Should().Be("Carrie-Anne Moss");
    }
    [Fact]
    public void Should_Not_Convert_Arrays_Of_Value_Types()
    {
        var movie = new Dictionary<string, object> {
            { "Id", Guid.NewGuid() },
            { "Name", "The Matrix" },
            { "ReleaseDate", null },
            { "Actors", new List<string> {
                "Keanu Reeves",
                "Carrie-Anne Moss"
            }}
        };
        var result = movie.NormalizeValuesForNeo4j();
        var actors = result["Actors"] as IEnumerable<string>;
        actors.Should().HaveCount(2);
        actors.Should().Contain("Keanu Reeves");
        actors.Should().Contain("Carrie-Anne Moss");
    }
    #endregion
}