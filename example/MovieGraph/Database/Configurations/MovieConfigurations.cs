using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Interfaces;
using berrybeat.Neo4j.OGM.Models;
using MovieGraph.Database.Models;

namespace MovieGraph.Database.Configurations;

public class MovieConfigurations : INodeConfiguration<Movie> {
    public void Configure(NodeTypeBuilder<Movie> builder)
    {
        builder.Exclude(x => x.Actors);
        builder.Exclude(x => x.Director);
        builder.HasRelationWithSingle(x => x.Director, "DIRECTED", RelationDirection.In);
        builder.HasRelationWithMultiple(x => x.Actors, "ACTED_IN", RelationDirection.In);
    }
}