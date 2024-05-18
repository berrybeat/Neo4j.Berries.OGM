namespace Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;


public class Car : IResource
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public string Brand { get; set; }
}