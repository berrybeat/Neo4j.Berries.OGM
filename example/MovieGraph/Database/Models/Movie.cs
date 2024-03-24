using Microsoft.AspNetCore.SignalR;

namespace MovieGraph.Database.Models;

public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Released { get; set; }
    public string Tagline { get; set; }
    public Person Director { get; set; }
    public List<Person> Actors { get; set; }
}