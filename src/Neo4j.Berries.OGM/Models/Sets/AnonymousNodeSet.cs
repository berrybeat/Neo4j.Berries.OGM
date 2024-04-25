using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
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
    internal CreateCommand CreateCommand { get; }
    public DatabaseContext DatabaseContext { get; }
    public NodeConfiguration NodeConfig { get; }
    public string Key { get; }
    private string Label { get; }
    private string UnwindVariable { get; }
    private int NodeSetIndex { get; }
    #endregion
    public IEnumerable<object> Nodes { get; private set; } = [];
    public NodeSet(string label, NodeConfiguration nodeConfiguration, int nodeSetIndex, DatabaseContext databaseContext, StringBuilder cypherBuilder)
    {
        NodeSetIndex = nodeSetIndex;
        NodeConfig = nodeConfiguration;
        DatabaseContext = databaseContext;
        CreationCypherBuilder = cypherBuilder;
        Key = $"anonymousList_{NodeSetIndex}";
        Label = label;
        UnwindVariable = $"{JsonNamingPolicy.CamelCase.ConvertName(label)}_{NodeSetIndex}";
        CreateCommand = new CreateCommand(nodeSetIndex, UnwindVariable, nodeConfiguration, cypherBuilder);
    }
    public void Add(Dictionary<string, object> node)
    {
        if (!Nodes.Any())
        {
            //e.g. UNWIND $people as person_0
            CreationCypherBuilder.AppendLine($"UNWIND ${Key} as {UnwindVariable}");
        }
        Nodes = Nodes.Append(node.NormalizeValuesForNeo4j());
        CreateCommand.Add(node);
    }
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
        Nodes = [];
    }
    public void BuildCypher()
    {
        CreateCommand.GenerateCypher(Label);
    }
}
