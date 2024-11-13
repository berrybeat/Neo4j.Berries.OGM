using Bogus;
using Bogus.DataSets;
using FluentAssertions;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Tests.Common;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Neo4j.Berries.OGM.Tests.Models
{
    public class RelationMappingTests() : TestBase()
    {
        private void AddMovieWithRelations(List<string> names, bool withDirector = false, bool withActors = false, int emptyDirectorEvery = 0)
        {
            TestGraphContext.Database.BeginTransaction(async () =>
            {
                ClearDatabase();
                Neo4jSingletonContext.Configs["Movie"].ExcludedProperties.Clear();

                List<Mocks.Models.Person> actors = [];

                if (withActors)
                {
                    actors = new Faker<Mocks.Models.Person>()
                    .RuleFor(x => x.Id, x => Guid.NewGuid())
                    .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                    .RuleFor(x => x.LastName, f => f.Person.LastName)
                    .Generate(3);
                }

                List<Mocks.Models.Movie> movies = [];
                int i = 0;
                foreach (var name in names)
                {
                    var fake = new Faker<Movie>()
                        .RuleFor(x => x.Id, x => Guid.NewGuid())
                        .RuleFor(x => x.Name, name)
                        .RuleFor(x => x.Director, f => !withDirector || (emptyDirectorEvery > 0 && i % emptyDirectorEvery == 0) ?
                                null : new Mocks.Models.Person
                                {
                                    Id = Guid.NewGuid(),
                                    FirstName = f.Name.FirstName(),
                                    LastName = f.Name.LastName()
                                })
                        .RuleFor(x => x.Actors, !withDirector ? [] : actors);


                    var movie = fake.Generate();
                    TestGraphContext.Movies.Add(movie);
                    i++;
                }

                await TestGraphContext.SaveChangesAsync();
            });
        }
        [Fact]
        public async void Should_Map_Included_Relations_In_FirstOrDefault()
        {
            var name = "Saving Private Ryan";
            AddMovieWithRelations([name], true, true);

            var query = TestGraphContext
                .Movies
                .Match(x => x.Where(y => y.Name, name))
                .WithRelation(x => x.Director)
                .WithRelation(x => x.Actors);
            var res = await query.FirstOrDefaultAsync();
            res.Director.Should().NotBeNull();
            res.Actors.Should().HaveCount(3);
        }

        [Fact]
        public async void Should_Map_Included_Relations_In_List()
        {
            AddMovieWithRelations(["Pulp Fiction", "The Big Short", "Interstellar"], true, true);

            var query = TestGraphContext
                .Movies
                .Match()
                .WithRelation(x => x.Director)
                .WithRelation(x => x.Actors);
            var movies = await query.ToListAsync();
            movies.Should().HaveCount(3);
            movies.Where(x => x.Director == null).Should().BeEmpty();
            movies.ForEach(x => x.Actors.Should().HaveCount(3));
        }

        [Fact]
        public async void Should_Not_Return_Null_Relations_On_WithRelation()
        {
            AddMovieWithRelations(["Pulp Fiction", "The Big Short", "Interstellar"], true, false, 2);

            var query = TestGraphContext
                .Movies
                .Match()
                .WithRelation(x => x.Director);
            var movies = await query.ToListAsync();
            movies.Should().HaveCount(1);
            movies.Where(x => x.Director == null).Should().BeEmpty();
        }

        [Fact]
        public async void Should_Return_Null_Relations_On_WithOptionalRelation()
        {
            AddMovieWithRelations(["Pulp Fiction", "The Big Short", "Interstellar"], true, false, 2);

            var query = TestGraphContext
                .Movies
                .Match()
                .WithOptionalRelation(x => x.Director);
            var movies = await query.ToListAsync();
            movies.Should().HaveCount(3);
            movies.Where(x => x.Director == null).Should().HaveCount(2);
        }
    }
}
