namespace Neo4j.Berries.OGM.Interfaces;


public interface INodeSet
{
    void Reset();
    void BuildCypher();
    string Name { get; }
    IEnumerable<object> MergeNodes { get; }
    IEnumerable<object> NewNodes { get; }
}