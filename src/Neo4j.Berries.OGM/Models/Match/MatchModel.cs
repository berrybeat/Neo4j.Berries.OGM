using System.Text;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Queries;

namespace Neo4j.Berries.OGM.Models.Match;

internal class MatchModel(string startNodeLabel, Eloquent eloquent, int index) : IMatch
{
    public string StartNodeAlias => $"l{index}";
    public Dictionary<string, object> GetParameters()
    {
        return eloquent?.QueryParameters ?? [];
    }

    public IMatch ToCypher(StringBuilder builder)
    {
        if (eloquent != null)
            builder.AppendLine($"MATCH ({StartNodeAlias}:{startNodeLabel} WHERE {eloquent.ToCypher(StartNodeAlias)})");
        else
            builder.AppendLine($"MATCH ({StartNodeAlias}:{startNodeLabel})");
        return this;
    }
}


internal class MatchModel<TNode>(Eloquent<TNode> eloquent, int index) : MatchModel(typeof(TNode).Name, eloquent, index)
where TNode : class { }