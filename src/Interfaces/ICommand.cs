namespace berrybeat.Neo4j.OGM.Interfaces;

public interface ICommand
{
    Dictionary<string, object> Parameters { get; set; }
}