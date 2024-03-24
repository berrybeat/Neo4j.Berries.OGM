using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Interfaces;
using berrybeat.Neo4j.OGM.Models;
using berrybeat.Neo4j.OGM.Models.Config;
using berrybeat.Neo4j.OGM.Tests.Mocks.Models;

namespace berrybeat.Neo4j.OGM.Tests.Mocks.Configurations;

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
        builder.Include(x => x.Id);
        builder.Include(x => x.FirstName, x => x.LastName);
        builder.Include(x => x.Age);
    }
}