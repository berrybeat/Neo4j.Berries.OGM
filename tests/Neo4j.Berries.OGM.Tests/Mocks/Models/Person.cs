using Neo4j.Berries.OGM.Models;
using Neo4j.Berries.OGM.Tests.Mocks.Models.Relations;

namespace Neo4j.Berries.OGM.Tests.Mocks.Models;


public class Person
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public DateTime? BirthDate { get; set; }
    public List<Movie> MoviesAsActor { get; set; } = [];
    public List<Movie> MoviesAsDirector { get; set; } = [];
    public List<Person> Friends { get; set; } = [];
    public Relations<Won, Award> Awards { get; set; } = [];
}