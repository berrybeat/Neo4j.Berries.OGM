namespace MovieGraph.Database.Models;

public class Person
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Movie> DirectedMovies { get; set; }
    public List<Movie> ActedInMovies { get; set; }
}