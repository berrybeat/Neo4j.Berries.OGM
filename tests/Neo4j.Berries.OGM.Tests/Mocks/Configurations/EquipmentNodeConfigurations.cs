using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Tests.Mocks.Models;

namespace Neo4j.Berries.OGM.Tests.Mocks.Configurations;

public class EquipmentNodeConfigurations : INodeConfiguration<Equipment>
{
    public void Configure(NodeTypeBuilder<Equipment> builder)
    {
        
    }
}