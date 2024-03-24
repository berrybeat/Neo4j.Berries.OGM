using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Interfaces;
using berrybeat.Neo4j.OGM.Models.Config;
using berrybeat.Neo4j.OGM.Tests.Mocks.Models;

namespace berrybeat.Neo4j.OGM.Tests.Mocks.Configurations;

public class MovieNodeConfiguration : INodeConfiguration<Movie>
{
    public void Configure(NodeTypeBuilder<Movie> builder)
    {
        builder.HasRelationWithMultiple(x => x.Actors, "ACTED_IN", RelationDirection.In)
            .OnMerge()
            .Include(x => x.Id);
        builder.HasRelationWithSingle(x => x.Director, "DIRECTED", RelationDirection.In)
            .OnMerge()
            .Include(x => x.Id);
        builder.Include(x => x.Id);
        builder.Include(x => x.Name);
        builder.Include(x => x.Year);
    }
}