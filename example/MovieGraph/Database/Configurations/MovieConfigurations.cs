using MovieGraph.Database.Models;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace MovieGraph.Database.Configurations;

public class MovieConfigurations : INodeConfiguration<Movie>
{
    public void Configure(NodeTypeBuilder<Movie> builder)
    {
        builder.HasRelationWithSingle(x => x.Director, new("DIRECTED", RelationDirection.In)
        {
            KeepHistory = true
        });
        builder.HasRelationWithMultiple(x => x.Actors, "ACTED_IN", RelationDirection.In);
        builder.HasIdentifier(x => x.Id);
    }
}