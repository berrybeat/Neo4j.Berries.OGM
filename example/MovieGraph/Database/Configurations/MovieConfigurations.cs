using MovieGraph.Database.Models;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace MovieGraph.Database.Configurations;

public class MovieConfigurations : INodeConfiguration<Movie> {
    public void Configure(NodeTypeBuilder<Movie> builder)
    {
        builder.HasRelationWithSingle(x => x.Director, "DIRECTED", RelationDirection.In);
        builder.HasRelationWithMultiple(x => x.Actors, "ACTED_IN", RelationDirection.In);
    }
}