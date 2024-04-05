using System.Text;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Queries;

namespace Neo4j.Berries.OGM.Models.Match;

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