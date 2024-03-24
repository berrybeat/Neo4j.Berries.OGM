using System.Text;
using berrybeat.Neo4j.OGM.Interfaces;

namespace berrybeat.Neo4j.OGM.Models.Match;

internal class MatchModel<TNode>(Eloquent<TNode> eloquent, int index) : IMatch
where TNode : class
{
    private string StartNodeLabel => typeof(TNode).Name;

    public string StartNodeAlias => $"l{index}";
    public IMatch ToCypher(StringBuilder cypherBuilder)
    {
        if(eloquent != null)
            cypherBuilder.AppendLine($"MATCH ({StartNodeAlias}:{StartNodeLabel} WHERE {eloquent.ToCypher(StartNodeAlias)})");
        else 
            cypherBuilder.AppendLine($"MATCH ({StartNodeAlias}:{StartNodeLabel})");
        return this;
    }
    public Dictionary<string, object> GetParameters()
    {
        return eloquent?.QueryParameters ?? [];
    }
    
}