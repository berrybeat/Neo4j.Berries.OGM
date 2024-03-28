using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Bogus;
using Person = Neo4j.Berries.OGM.Tests.Mocks.Models.Person;
using Neo4j.Berries.OGM.Tests.Mocks.Enums;

namespace Neo4j.Berries.OGM.Tests.Mocks;

public class Seed(ApplicationGraphContext graphContext)
{
    public async Task ExecuteFullAsync()
    {
        var fullMovieFaker = new Faker<Movie>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.Year, f => f.Date.Past().Year)
            .RuleFor(x => x.Actors, GetPeople(5))
            .RuleFor(x => x.Director, GetPeople(1).First())
            .RuleFor(x => x.Equipments, GetEquipments(2));
        var withActorsFaker = new Faker<Movie>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.Year, f => f.Date.Past().Year)
            .RuleFor(x => x.Actors, GetPeople(5));
        var movies = fullMovieFaker.Generate(5).Concat(withActorsFaker.Generate(5));
        graphContext.Movies.AddRange(movies);
        await graphContext.SaveChangesAsync();
    }
    public List<Person> GetPeople(int count = 10)
    {
        var faker = new Faker<Person>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Age, f => f.Random.Number(18, 99));
        return faker.Generate(count);
    }

    public List<Equipment> GetEquipments(int count = 10)
    {
        var faker = new Faker<Equipment>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.Type, f => f.PickRandom<EquipmentType>());
        return faker.Generate(count);
    }
}