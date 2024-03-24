using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Models;

public class NodeSet<TNode>(int nodeSetIndex, DatabaseContext databaseContext, StringBuilder cypherBuilder) : INodeSet
where TNode : class
{
    private readonly int _nodeSetIndex = nodeSetIndex;

    public DatabaseContext InternalDatabaseContext { get; } = databaseContext;
    internal IDriver Neo4jDriver { get; }
    internal StringBuilder CreationCypherBuilder { get; } = cypherBuilder;
    public IList<ICommand> CreateCommands { get; private set; } = [];

    /// <summary>
    /// Adds a node to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public TNode Add(TNode node)
    {
        CreateCommands.Add(new CreateCommand<TNode>(node, CreateCommands.Count, _nodeSetIndex, CreationCypherBuilder));
        return node;
    }
    /// <summary>
    /// Adds a range of nodes to the set. Only after calling the `SaveChangesAsync` the added objects will be transferred to the database.
    /// </summary>
    public void AddRange(IEnumerable<TNode> nodes)
    {
        (CreateCommands as List<ICommand>).AddRange(
            nodes.Select((node, index) => new CreateCommand<TNode>(node, CreateCommands.Count, _nodeSetIndex, CreationCypherBuilder))
        );
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    /// <param name="eloquent">The eloquent query to find nodes in the database</param>
    public NodeQuery<TNode> Match(Func<Eloquent<TNode>, Eloquent<TNode>> eloquent)
    {
        var query = new NodeQuery<TNode>(eloquent(new Eloquent<TNode>(0)), InternalDatabaseContext);
        return query;
    }
    /// <summary>
    /// Starts a query to find nodes in the database and execute Update, Connect, Disconnect on the found relations/nodes.
    /// </summary>
    public NodeQuery<TNode> Match()
    {
        var query = new NodeQuery<TNode>(null, InternalDatabaseContext);
        return query;
    }
    /// <summary>
    /// This will be called automatically when calling `SaveChangesAsync`.
    /// </summary>
    public void Reset()
    {
        CreateCommands = [];
    }
}