namespace Neo4j.Berries.OGM.Interfaces;


public interface INodeSet
{
    IList<ICommand> CreateCommands { get; }
    void Reset();
}