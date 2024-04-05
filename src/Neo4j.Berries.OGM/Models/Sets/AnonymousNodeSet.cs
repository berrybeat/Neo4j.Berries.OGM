using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Queries;

namespace Neo4j.Berries.OGM.Models.Sets;

public class NodeSet(string label, NodeConfiguration nodeConfiguration, int nodeSetIndex, DatabaseContext databaseContext, StringBuilder cypherBuilder) : INodeSet
{
    private readonly int _nodeSetIndex = nodeSetIndex;
    public DatabaseContext InternalDatabaseContext { get; } = databaseContext;
    internal StringBuilder CreationCypherBuilder { get; } = cypherBuilder;
    public IList<ICommand> CreateCommands { get; private set; } = [];
    public void Add(Dictionary<string, object> node)
    {
        var command = new CreateCommand(
            node: node,
            label: label,
            nodeConfig: nodeConfiguration,
            itemIndex: CreateCommands.Count,
            nodeSetIndex: _nodeSetIndex,
            cypherBuilder: CreationCypherBuilder,
            anonymous: true);
        CreateCommands.Add(command);
    }
    public void AddRange(IEnumerable<Dictionary<string, object>> node)
    {
        (CreateCommands as List<ICommand>).AddRange(
            node.Select((node, index) => new CreateCommand(
                node: node,
                label: label,
                nodeConfig: nodeConfiguration,
                itemIndex: CreateCommands.Count,
                nodeSetIndex: _nodeSetIndex,
                cypherBuilder: CreationCypherBuilder,
                anonymous: true))
        );
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    /// <param name="eloquent">The eloquent query to find nodes in the database</param>
    public NodeQuery Match(Func<Eloquent, Eloquent> eloquent)
    {
        var query = new NodeQuery(label, nodeConfiguration, eloquent(new Eloquent(0)), InternalDatabaseContext);
        return query;
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    public NodeQuery Match()
    {
        var query = new NodeQuery(label, nodeConfiguration, null, InternalDatabaseContext);
        return query;
    }
    public void Reset()
    {
        CreateCommands = [];
    }
}
