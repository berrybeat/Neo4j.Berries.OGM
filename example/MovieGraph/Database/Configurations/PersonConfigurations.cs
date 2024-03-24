using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Interfaces;
using berrybeat.Neo4j.OGM.Models;
using MovieGraph.Database.Models;

namespace MovieGraph.Database.Configurations;

public class PersonConfigurations : INodeConfiguration<Person>
{
    public void Configure(NodeTypeBuilder<Person> builder)
    {
        builder.Exclude(x => x.ActedInMovies);
        builder.Exclude(x => x.DirectedMovies);
        builder.HasRelationWithMultiple(x => x.DirectedMovies, "DIRECTED", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.ActedInMovies, "ACTED_IN", RelationDirection.Out);
    }
}