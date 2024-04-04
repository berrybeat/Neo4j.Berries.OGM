using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;

namespace Neo4j.Berries.OGM.Models.Sets;

public class NodeSet(string label, int nodeSetIndex, DatabaseContext databaseContext, StringBuilder cypherBuilder) : INodeSet
{
    private readonly int _nodeSetIndex = nodeSetIndex;
    public DatabaseContext InternalDatabaseContext { get; } = databaseContext;
    internal StringBuilder CreationCypherBuilder { get; } = cypherBuilder;
    public IList<ICommand> CreateCommands { get; private set; } = [];
    public void Reset()
    {
        throw new NotImplementedException();
    }
}
