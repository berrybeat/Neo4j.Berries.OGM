using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Utils;

internal static class RelationUtils
{
    internal static string Format(this IRelationConfiguration relation, string alias = null, string condition = null)
    {
        if (relation.Direction == RelationDirection.Out)
        {
            return $"-[{alias}:{relation.Label}{condition}]->";
        }
        else
        {
            return $"<-[{alias}:{relation.Label}{condition}]-";
        }
    }

    internal static IEnumerable<KeyValuePair<string, object>> GetRelations(this IEnumerable<Dictionary<string, object>> input, NodeConfiguration nodeConfig, Func<object, bool> checker)
    {
        return input
            .SelectMany(x => x)
            .Where(x => x.Value != null)
            .Where(x => nodeConfig.Relations.ContainsKey(x.Key))
            .Where(x => checker(x.Value));
    }
}