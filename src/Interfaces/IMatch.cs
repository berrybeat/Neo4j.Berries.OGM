using System.Text;

namespace berrybeat.Neo4j.OGM.Interfaces;

public interface IMatch
{
    string StartNodeAlias { get; }
    string RelationAlias => null;
    IMatch ToCypher(StringBuilder builder);
    Dictionary<string, object> GetParameters();
}