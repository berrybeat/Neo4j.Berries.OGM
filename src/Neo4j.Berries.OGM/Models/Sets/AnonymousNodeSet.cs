using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Queries;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

public class NodeSet : INodeSet
{
    #region Constructor parameters
    internal StringBuilder CreationCypherBuilder { get; }
    public DatabaseContext DatabaseContext { get; }
    public NodeConfiguration NodeConfig { get; }
    //This is the name of the NodeSet
    public string Name { get; }
    private string Label { get; }
    private int NodeSetIndex { get; }
    #endregion
    internal Node MergeNode;
    internal Node NewNode;
    public IEnumerable<object> MergeNodes { get; private set; } = [];
    public IEnumerable<object> NewNodes { get; private set; } = [];
    public NodeSet(string label, NodeConfiguration nodeConfiguration, int nodeSetIndex, DatabaseContext databaseContext, StringBuilder cypherBuilder)
    {
        NodeSetIndex = nodeSetIndex;
        NodeConfig = nodeConfiguration;
        DatabaseContext = databaseContext;
        CreationCypherBuilder = cypherBuilder;
        Name = $"anonymousList_{NodeSetIndex}";
        Label = label;
    }

    /// <summary>
    /// This method will try to merge a whole path of nodes and relations. It can be slower than the Add method.
    /// </summary>
    public void Merge(Dictionary<string, object> node)
    {
        MergeNode ??= new(Label);
        MergeNodes = MergeNodes.Append(node.NormalizeValuesForNeo4j());
    }
    /// <summary>
    /// This method will try to merge a whole path of nodes and relations. It can be slower than the Add method.
    /// </summary>
    public void MergeRange(IEnumerable<Dictionary<string, object>> nodes)
    {
        foreach (var node in nodes) Merge(node);
    }

    /// <summary>
    /// This method anonymously add a new node to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public void Add(Dictionary<string, object> node)
    {
        NewNode ??= new(Label);
        NewNodes = NewNodes.Append(node.NormalizeValuesForNeo4j());
    }
    /// <summary>
    /// This method anonymously add a collection of nodes to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public void AddRange(IEnumerable<Dictionary<string, object>> nodes)
    {
        foreach (var node in nodes) Add(node);
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    /// <param name="eloquent">The eloquent query to find nodes in the database</param>
    public NodeQuery Match(Func<Eloquent, Eloquent> eloquent)
    {
        var query = new NodeQuery(
            startNodeLabel: Label,
            eloquent: eloquent(new Eloquent(0)),
            databaseContext: DatabaseContext,
            nodeConfiguration: NodeConfig);
        return query;
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    public NodeQuery Match()
    {
        var query = new NodeQuery(
            startNodeLabel: Label,
            eloquent: null,
            databaseContext: DatabaseContext,
            nodeConfiguration: NodeConfig);
        return query;
    }
    public void Reset()
    {
        MergeNodes = [];
        NewNodes = [];
        MergeNode = null;
        NewNode = null;
        CreationCypherBuilder.Clear();
    }
    public void BuildCypher()
    {
        MergeNode?.Merge(CreationCypherBuilder, $"${Name}_merges", NodeSetIndex);
        NewNode?.Create(CreationCypherBuilder, $"${Name}_creates", NodeSetIndex);
    }
}
