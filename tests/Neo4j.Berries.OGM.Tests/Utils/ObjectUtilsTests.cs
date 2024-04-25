using FluentAssertions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Tests.Utils;

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
    #endregion
}