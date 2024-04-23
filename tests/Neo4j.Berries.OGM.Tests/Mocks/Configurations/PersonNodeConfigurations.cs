using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Mocks.Configurations;

public class PersonNodeConfigurations : INodeConfiguration<Person>
{
    public void Configure(NodeTypeBuilder<Person> builder)
    {
        builder.HasRelationWithMultiple(x => x.MoviesAsActor, "ACTED_IN", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.MoviesAsDirector, "DIRECTED", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.Friends, "FRIENDS_WITH", RelationDirection.Out)
            .OnMerge()
            .Include(x => x.Id)
            .Include(x => x.Age);
        builder.HasRelationWithMultiple(x => x.Awards, "WON", RelationDirection.Out)
            .OnMerge()
            .Include(x => x.Name)
            .Include(x => x.Category);
        builder.Include(x => x.Id);
        builder.Include(x => x.FirstName, x => x.LastName);
        builder.Include(x => x.Age);
    }
}