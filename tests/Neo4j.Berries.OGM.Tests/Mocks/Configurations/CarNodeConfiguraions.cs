using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;

namespace Neo4j.Berries.OGM.Tests.Mocks.Configurations;

public class CarNodeConfiguration : INodeConfiguration<Car>
{
    public void Configure(NodeTypeBuilder<Car> builder)
    {
        builder.HasIdentifier(x => x.LicensePlate);
    }
}
