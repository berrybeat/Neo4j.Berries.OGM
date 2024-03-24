using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Bogus;

namespace Neo4j.Berries.OGM.Tests.Common;

public class FakeActors
{
    public static List<Mocks.Models.Person> GetActor(int actorsCount = 5)
    {
        var faker = new Faker<Mocks.Models.Person>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.FirstName, f => f.Name.FirstName())
            .RuleFor(m => m.LastName, f => f.Name.LastName())
            .RuleFor(m => m.Age, f => f.Random.Number(1, 100));
        return faker.Generate(actorsCount);
    }
}