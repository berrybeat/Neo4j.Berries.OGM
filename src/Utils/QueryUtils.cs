using System.Text;
using berrybeat.Neo4j.OGM.Interfaces;

namespace berrybeat.Neo4j.OGM.Utils;

internal static class QueryUtils
{
    internal static StringBuilder BuildAnyQuery(this StringBuilder builder, List<IMatch> matches)
    {
        return builder.AppendLines(
            $"WITH DISTINCT {matches.First().StartNodeAlias}",
            $"RETURN count({matches.First().StartNodeAlias}) > 0 as any"
        );
    }
    internal static StringBuilder BuildCountQuery(this StringBuilder builder, List<IMatch> matches)
    {
        return builder.AppendLines(
            $"WITH DISTINCT {matches.First().StartNodeAlias}",
            $"RETURN count({matches.First().StartNodeAlias}) as count"
        );
    }
    internal static StringBuilder BuildFirstOrDefaultQuery(this StringBuilder builder, List<IMatch> matches)
    {
        var key = matches.First().StartNodeAlias;
        return builder.AppendLines(
            $"WITH DISTINCT {key}",
            $"RETURN {key}"
        );
    }
    internal static StringBuilder BuildListQuery(this StringBuilder builder, List<IMatch> matches)
    {
        var key = matches.First().StartNodeAlias;
        return builder.AppendLines(
            $"WITH DISTINCT {key}",
            $"RETURN {key}"
        );
    }

    internal static void BuildConnectionRelation(this StringBuilder builder, IRelationConfiguration relationConfig, List<IMatch> matches)
    {
        builder.AppendLine($"CREATE ({matches.First().StartNodeAlias}){relationConfig.Format()}({matches.Last().StartNodeAlias})");
    }
}