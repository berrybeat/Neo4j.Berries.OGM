using System.Text;
using System.Text.Json;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Queries;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

public class NodeSet<TNode> : INodeSet
where TNode : class
{
    #region Constructor parameters
    internal StringBuilder CreationCypherBuilder { get; }
    internal CreateCommand CreateCommand { get; }
    public DatabaseContext DatabaseContext { get; }
    public NodeConfiguration NodeConfig { get; }
    public string Key { get; }
    private string UnwindVariable { get; }
    private int NodeSetIndex { get; }
    #endregion
    public IEnumerable<object> Nodes { get; private set; } = [];

    public NodeSet(int nodeSetIndex, string name, NodeConfiguration nodeConfig, DatabaseContext databaseContext, StringBuilder cypherBuilder)
    {
        NodeSetIndex = nodeSetIndex;
        NodeConfig = nodeConfig;
        DatabaseContext = databaseContext;
        CreationCypherBuilder = cypherBuilder;
        Key = $"{JsonNamingPolicy.CamelCase.ConvertName(name)}";
        //e.g. person_0
        UnwindVariable = $"uw_{JsonNamingPolicy.CamelCase.ConvertName(typeof(TNode).Name)}_{NodeSetIndex}";
        CreateCommand = new CreateCommand(nodeSetIndex, UnwindVariable, nodeConfig, cypherBuilder);
    }

    /// <summary>
    /// Adds a node to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public void Add(TNode node)
    {
        if (!Nodes.Any())
        {
            //e.g. UNWIND $people as person_0
            CreationCypherBuilder.AppendLine($"UNWIND ${Key} as {UnwindVariable}");
        }
        Nodes = Nodes.Append(node.ToDictionary(Neo4jSingletonContext.Configs));
        CreateCommand.Add(node);
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
        var query = new NodeQuery<TNode>(eloquent(new Eloquent<TNode>(0)), DatabaseContext);
        return query;
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    public NodeQuery<TNode> Match()
    {
        var query = new NodeQuery<TNode>(null, DatabaseContext);
        return query;
    }
    /// <summary>
    /// This will be called automatically when calling `SaveChangesAsync`.
    /// </summary>
    public void Reset()
    {
        Nodes = [];
    }

    public void BuildCypher()
    {
        CreateCommand.GenerateCypher(typeof(TNode).Name);
    }
}