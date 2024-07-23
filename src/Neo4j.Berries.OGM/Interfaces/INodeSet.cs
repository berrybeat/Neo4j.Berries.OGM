namespace Neo4j.Berries.OGM.Interfaces;


public interface INodeSet
{
    void Reset();
    void BuildCypher(Dictionary<string, object> parameters);
    string Name { get; }
    IEnumerable<object> MergeNodes { get; }
    IEnumerable<object> NewNodes { get; }
}