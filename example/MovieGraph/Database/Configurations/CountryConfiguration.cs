using MovieGraph.Database.Models;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace MovieGraph.Database.Configurations;

public class CountryConfiguration : INodeConfiguration<Country>
{
    public void Configure(NodeTypeBuilder<Country> builder)
    {
        builder.HasIdentifier(x => x.Code);
    }
}