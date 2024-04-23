
namespace Neo4j.Berries.OGM.Tests.Mocks.Models;

public class Movie
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ReleaseDate { get; set; }
    public List<Person> Actors { get; set; } = [];
    public Person Director { get; set; }
    public Location Location { get; set; }
    public List<Equipment> Equipments { get; set; }
}