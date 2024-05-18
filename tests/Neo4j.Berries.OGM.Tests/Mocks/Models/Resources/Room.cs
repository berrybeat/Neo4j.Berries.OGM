namespace Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;

public class Room : IResource
{
    public Guid Id { get; set; }
    public string Number { get; set; }
}