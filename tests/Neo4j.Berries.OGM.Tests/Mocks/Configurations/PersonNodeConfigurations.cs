using FluentAssertions.Equivalency.Tracing;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Mocks.Configurations;

public class PersonNodeConfigurations : INodeConfiguration<Person>
{
    public void Configure(NodeTypeBuilder<Person> builder)
    {
        builder.HasIdentifier(x => x.Id);
        builder.HasRelationWithSingle(x => x.Address, "LIVES_IN", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.MoviesAsActor, "ACTED_IN", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.MoviesAsDirector, "DIRECTED", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.Resources, "USES", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.Friends, "FRIENDS_WITH", RelationDirection.Out)
            .OnMerge()
            .Include(x => x.Id)
            .Include(x => x.Age);
        builder.Include(x => x.Id);
        builder.Include(x => x.FirstName, x => x.LastName);
        builder.Include(x => x.Age);
    }
}