namespace Neo4j.Berries.OGM.Tests.Mocks.Models.Resources;


public class Car : IResource
{
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public string Brand { get; set; }
}