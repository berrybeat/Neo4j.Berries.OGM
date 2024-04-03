using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Mocks.Configurations;

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
        builder.HasRelationWithSingle(x => x.Location, "FILMED_AT", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.Equipments, "USES", RelationDirection.Out);
        builder.Include(x => x.Id);
        builder.Include(x => x.Name);
        builder.Include(x => x.ReleaseDate);
    }
}