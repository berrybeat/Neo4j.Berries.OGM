namespace Neo4j.Berries.OGM.Tests.Mocks.Models;

public class Movie
{
    public Movie()
    {
        Actors = new List<Person>();
    }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Year { get; set; }
    public List<Person> Actors { get; set; }
    public Person Director { get; set; }
    public Location Location { get; set; }
}