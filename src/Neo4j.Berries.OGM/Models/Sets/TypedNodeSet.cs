using System.Text;
using System.Text.Json;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Queries;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

public class NodeSet<TNode>(int nodeSetIndex, string name, DatabaseContext databaseContext, StringBuilder cypherBuilder) : INodeSet
where TNode : class
{
    #region Constructor parameters
    //This is the name of the NodeSet.
    public string Name { get; } = $"{JsonNamingPolicy.CamelCase.ConvertName(name)}";
    #endregion
    public IEnumerable<object> MergeNodes { get; private set; } = [];
    public IEnumerable<object> NewNodes { get; private set; } = [];
    internal Node MergeNode;
    internal Node NewNode;

    /// <summary>
    /// This will try to merge every nodes and relations in a path and can be slower than the Add method.
    /// </summary>
    public void Merge(TNode node)
    {
        MergeNode ??= new(typeof(TNode).Name);
        var _node = node.ToDictionary(Neo4jSingletonContext.Configs);
        MergeNodes = MergeNodes.Append(_node);
    }
    /// <summary>
    /// This will try to merge every nodes and relations in a path and can be slower than the Add method.
    /// </summary>
    public void MergeRange(IEnumerable<TNode> nodes)
    {
        foreach (var node in nodes) Merge(node);
    }

    /// <summary>
    /// Adds a node to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public void Add(TNode node)
    {
        NewNode ??= new(typeof(TNode).Name);
        var _node = node.ToDictionary(Neo4jSingletonContext.Configs);
        NewNodes = NewNodes.Append(_node);
    }
    /// <summary>
    /// Adds a range of nodes to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public void AddRange(IEnumerable<TNode> nodes)
    {
        foreach (var node in nodes) Add(node);
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    /// <param name="eloquent">The eloquent query to find nodes in the database</param>
    public NodeQuery<TNode> Match(Func<Eloquent<TNode>, Eloquent<TNode>> eloquent)
    {
        var query = new NodeQuery<TNode>(eloquent(new Eloquent<TNode>(0)), databaseContext);
        return query;
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    public NodeQuery<TNode> Match()
    {
        var query = new NodeQuery<TNode>(null, databaseContext);
        return query;
    }
    /// <summary>
    /// This will be called automatically when calling `SaveChangesAsync`.
    /// </summary>
    public void Reset()
    {
        MergeNodes = [];
        NewNodes = [];
        MergeNode = null;
        NewNode = null;
        cypherBuilder.Clear();
    }

    public void BuildCypher()
    {
        MergeNode?.Consider(MergeNodes.Select(x => x as Dictionary<string, object>));
        MergeNode?.Merge(cypherBuilder, $"${Name}_merges", nodeSetIndex);
        NewNode?.Consider(NewNodes.Select(x => x as Dictionary<string, object>));
        NewNode?.Create(cypherBuilder, $"${Name}_creates", nodeSetIndex);
    }
}