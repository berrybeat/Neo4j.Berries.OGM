namespace Neo4j.Berries.OGM.Interfaces;

public interface ICommand
{
    Dictionary<string, object> Parameters { get; set; }
}