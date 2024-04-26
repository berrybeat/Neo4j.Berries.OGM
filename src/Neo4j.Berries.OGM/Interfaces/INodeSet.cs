namespace Neo4j.Berries.OGM.Interfaces;


public interface INodeSet
{
    void Reset();
    void BuildCypher();
    string Key { get; }
    IEnumerable<object> Nodes { get; }
}