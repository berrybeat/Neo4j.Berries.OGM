using MovieGraph.Database.Models;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace MovieGraph.Database.Configurations;

public class PersonConfigurations : INodeConfiguration<Person>
{
    public void Configure(NodeTypeBuilder<Person> builder)
    {
        builder.HasRelationWithMultiple(x => x.DirectedMovies, "DIRECTED", RelationDirection.Out);
        builder.HasRelationWithMultiple(x => x.ActedInMovies, "ACTED_IN", RelationDirection.Out);
    }
}