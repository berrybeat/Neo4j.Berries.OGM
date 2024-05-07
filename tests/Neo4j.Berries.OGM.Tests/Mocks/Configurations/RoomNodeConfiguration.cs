using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;

namespace Neo4j.Berries.OGM.Tests.Mocks.Configurations;

public class RoomNodeConfiguration : INodeConfiguration<Room>
{
    public void Configure(NodeTypeBuilder<Room> builder)
    {
        builder.HasIdentifier(x => x.Number);
    }
}