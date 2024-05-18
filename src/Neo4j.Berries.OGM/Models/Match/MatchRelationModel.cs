using System.Text;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Queries;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Match;

internal class MatchRelationModel(IMatch startMatch, IRelationConfiguration relationConfig, Eloquent eloquent, int index) : IMatch
{
    private IMatch StartMatch { get; set; } = startMatch;
    private IRelationConfiguration RelationConfig { get; set; } = relationConfig;
    private Eloquent EndNodeEloquent { get; set; } = eloquent;
    public string StartNodeAlias => StartMatch.StartNodeAlias;
    public string EndNodeAlias => $"l{index}";
    public string RelationAlias => $"r{index}";

    public string EndNodeLabel { get; } = relationConfig.EndNodeLabels.Length > 1 ? null : relationConfig.EndNodeLabels[0];

    public IMatch ToCypher(StringBuilder cypherBuilder)
    {
        var endNodeStatement = string.Join(
            ':',
            new List<string>() { EndNodeAlias, EndNodeLabel }.Where(x => x != null)
        );
        if (EndNodeEloquent != null)
            cypherBuilder.AppendLine($"MATCH ({StartMatch.StartNodeAlias}){RelationConfig.Format(RelationAlias)}({endNodeStatement} WHERE {EndNodeEloquent.ToCypher(EndNodeAlias)})");
        else
            cypherBuilder.AppendLine($"MATCH ({StartMatch.StartNodeAlias}){RelationConfig.Format(RelationAlias)}({endNodeStatement})");
        return this;
    }
    public Dictionary<string, object> GetParameters()
    {
        return EndNodeEloquent?.QueryParameters ?? [];
    }
}

internal class MatchRelationModel<TEndNode>(IMatch startMatch, IRelationConfiguration relationConfig, Eloquent<TEndNode> eloquent, int index) : MatchRelationModel(startMatch, relationConfig, eloquent, index)
where TEndNode : class
{ }