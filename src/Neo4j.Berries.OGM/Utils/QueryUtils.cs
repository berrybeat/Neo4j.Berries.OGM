using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Match;

namespace Neo4j.Berries.OGM.Utils;

internal static class QueryUtils
{
    internal static StringBuilder BuildLockQuery(this StringBuilder builder, List<IMatch> matches)
    {
        return builder.AppendLines(
            $"SET {matches.First().StartNodeAlias}._LOCK_ = true"
        );
    }
    internal static StringBuilder BuildUnlockQuery(this StringBuilder builder, List<IMatch> matches)
    {
        return builder.AppendLines(
            $"REMOVE {matches.First().StartNodeAlias}._LOCK_"
        );
    }
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
        var withKey = $"{matches.First().StartNodeAlias}";
        var returnKey = withKey;
        foreach (var relation in matches.Skip(1).OfType<MatchRelationModel>())
        {
            withKey += $", {relation.EndNodeAlias}";
            if (relation.IsCollection)
            {
                returnKey += $", collect({relation.EndNodeAlias}) as {CollectionAlias(relation.EndNodeAlias)}";
            }
            else
            {
                returnKey = withKey;
            }
        }
        return builder.AppendLines(
            $"WITH DISTINCT {withKey}",
            $"RETURN {returnKey}"
        );
    }
    internal static StringBuilder BuildListQuery(this StringBuilder builder, List<IMatch> matches)
    {
        var withKey = $"{matches.First().StartNodeAlias}";
        var returnKey = withKey;
        foreach (var relation in matches.Skip(1).OfType<MatchRelationModel>())
        {
            withKey += $", {relation.EndNodeAlias}";
            if (relation.IsCollection)
            {
                returnKey += $", collect({relation.EndNodeAlias}) as {CollectionAlias(relation.EndNodeAlias)}";
            }
            else
            {
                returnKey = withKey;
            }
        }
        return builder.AppendLines(
            $"WITH DISTINCT {withKey}",
            $"RETURN {returnKey}"
        );
    }

    internal static void BuildConnectionRelation(this StringBuilder builder, IRelationConfiguration relationConfig, List<IMatch> matches)
    {
        builder.AppendLine($"CREATE ({matches.First().StartNodeAlias}){relationConfig.Format("r0")}({matches.Last().StartNodeAlias})");
        var timestampConfig = Neo4jSingletonContext.TimestampConfiguration;
        if (timestampConfig.Enabled)
        {
            if (timestampConfig.EnforceModifiedTimestampKey)
                builder.AppendLine($"SET r0.{timestampConfig.CreatedTimestampKey} = timestamp(), r0.{timestampConfig.ModifiedTimestampKey} = timestamp()");
            else
                builder.AppendLine($"SET r0.{timestampConfig.CreatedTimestampKey} = timestamp()");

        }
    }

    internal static string CollectionAlias(string alias) => $"coll_{alias}";
}