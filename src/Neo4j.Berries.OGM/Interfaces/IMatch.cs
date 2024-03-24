using System.Text;

namespace Neo4j.Berries.OGM.Interfaces;

public interface IMatch
{
    string StartNodeAlias { get; }
    string RelationAlias => null;
    IMatch ToCypher(StringBuilder builder);
    Dictionary<string, object> GetParameters();
}