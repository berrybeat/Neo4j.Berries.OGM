using System.Diagnostics;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using MovieGraph.Database;
using MovieGraph.Database.Models;

namespace MovieGraph.Controllers;


[ApiController]
[Route("movies")]
public class MoviesController(ApplicationGraphContext graphContext) : ControllerBase
{
    [HttpPost("create")]
    public async Task<long> Create()
    {
        var fake = new Faker<Movie>()
            .RuleFor(x => x.Id, x => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.Lorem.Sentence())
            .RuleFor(x => x.Released, f => f.Date.Past().Year)
            .RuleFor(x => x.Tagline, f => f.Lorem.Sentence())
            .RuleFor(x => x.Director, f => new Database.Models.Person
            {
                Id = Guid.NewGuid(),
                FirstName = f.Name.FirstName(),
                LastName = f.Name.LastName()
            });
        var stopwatch = new Stopwatch();
        var movies = fake.Generate(1);
        stopwatch.Start();
        graphContext.Movies.MergeRange(movies);
        await graphContext.SaveChangesAsync();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
    [HttpPut("update")]
    public IActionResult Update(Movie movie)
    {
        graphContext.Movies.Merge(movie);
        graphContext.SaveChanges();
        return Ok();
    }
}