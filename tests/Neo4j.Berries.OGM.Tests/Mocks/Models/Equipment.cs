using Neo4j.Berries.OGM.Tests.Mocks.Enums;

namespace Neo4j.Berries.OGM.Tests.Mocks.Models;

public class Equipment
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public EquipmentType Type { get; set; }
}