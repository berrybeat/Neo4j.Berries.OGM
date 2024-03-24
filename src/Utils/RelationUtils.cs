using berrybeat.Neo4j.OGM.Enums;

namespace berrybeat.Neo4j.OGM.Utils;

internal static class RelationUtils
{
    internal static string Format(this IRelationConfiguration relation, string alias = null)
    {
        if (relation.Direction == RelationDirection.Out)
        {
            return $"-[{alias}:{relation.Label}]->";
        }
        else
        {
            return $"<-[{alias}:{relation.Label}]-";
        }
    }
}