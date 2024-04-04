using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Models.Sets;

public class NodeSet(string label, int nodeSetIndex, DatabaseContext databaseContext, StringBuilder cypherBuilder) : INodeSet
{
    private readonly int _nodeSetIndex = nodeSetIndex;
    public DatabaseContext InternalDatabaseContext { get; } = databaseContext;
    internal StringBuilder CreationCypherBuilder { get; } = cypherBuilder;
    public IList<ICommand> CreateCommands { get; private set; } = [];
    public void Add(object node)
    {
        var command = new CreateCommand(
            node: node,
            label: label,
            nodeConfig: new NodeConfiguration(),
            itemIndex: CreateCommands.Count(),
            nodeSetIndex: _nodeSetIndex,
            cypherBuilder: CreationCypherBuilder,
            anonymous: true);
        CreateCommands.Add(command);
    }
    public void AddRange(IEnumerable<object> node)
    {
        (CreateCommands as List<ICommand>).AddRange(
            node.Select((node, index) => new CreateCommand(
                node: node,
                label: label,
                nodeConfig: new NodeConfiguration(),
                itemIndex: CreateCommands.Count(),
                nodeSetIndex: _nodeSetIndex,
                cypherBuilder: CreationCypherBuilder,
                anonymous: true))
        );
    }
    public void Reset()
    {
        CreateCommands = [];
    }
}
