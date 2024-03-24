using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Utils;

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