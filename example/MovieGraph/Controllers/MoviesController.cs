using System.Diagnostics;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using MovieGraph.Database;
using MovieGraph.Database.Models;
using Neo4j.Berries.OGM.Models.Queries;

namespace MovieGraph.Controllers;


[ApiController]
[Route("movies")]
public class MoviesController(ApplicationGraphContext graphContext) : ControllerBase
{
    [HttpPost("create")]
    public async Task<long> Create()
    {
        var actors = new Faker<Database.Models.Person>()
            .RuleFor(x => x.Id, x => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .Generate(20);

        var fake = new Faker<Movie>()
            .RuleFor(x => x.Id, x => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.Lorem.Sentence())
            .RuleFor(x => x.Released, f => f.Date.Past().Year)
            .RuleFor(x => x.Tagline, f => f.Lorem.Sentence())
            // About 30% of Movie nodes will not have directors; for testing OPTIONAL MATCH
            .RuleFor(x => x.Director, f => f.Random.Int(1, 10) <= 3 ? null : new Database.Models.Person  
            {
                Id = Guid.NewGuid(),
                FirstName = f.Name.FirstName(),
                LastName = f.Name.LastName()
            })
            .RuleFor(x => x.Actors, f => f.PickRandom(actors, f.Random.Int(0, 10)).ToList());

        var stopwatch = new Stopwatch();
        var movies = fake.Generate(10);
        stopwatch.Start();
        graphContext.People.AddRange(actors);
        await graphContext.SaveChangesAsync();
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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var movies = await graphContext.Movies
            .Match()
            .WithOptionalRelation(x => x.Director)
            .WithOptionalRelation(x => x.Actors)
            .ToListAsync();
        return Ok(movies);
    }

    [HttpGet("bytitle/{title}")]
    public async Task<IActionResult> GetByTitle(string title)
    {
        var movies = await graphContext.Movies
            .Match(x => x.WhereContains(x => x.Title, title))
            .WithOptionalRelation(x => x.Director)
            .WithOptionalRelation(x => x.Actors)
            .ToListAsync();
        return Ok(movies);
    }
}