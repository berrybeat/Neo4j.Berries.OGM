using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Bogus;

namespace Neo4j.Berries.OGM.Tests.Common;

public class FakeMovies
{
    public static List<Movie> GetMovie(int moviesCount = 10, int actorsCount = 5)
    {
        var faker = new Faker<Movie>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.Name, f => f.Random.Words(1))
            .RuleFor(m => m.ReleaseDate, f => f.Date.Past())
            .RuleFor(m => m.Actors, f => FakeActors.GetActor(actorsCount));
        return faker.Generate(moviesCount);
    }
}