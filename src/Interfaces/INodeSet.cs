namespace berrybeat.Neo4j.OGM.Interfaces;


public interface INodeSet
{
    IList<ICommand> CreateCommands { get; }
    void Reset();
}