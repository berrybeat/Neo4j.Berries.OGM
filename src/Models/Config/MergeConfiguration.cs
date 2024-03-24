using System.Linq.Expressions;

namespace berrybeat.Neo4j.OGM.Models.Config;

public class MergeConfiguration<TNode>
where TNode : class
{
    internal IEnumerable<string> IncludedProperties { get; private set; } = [];

    public MergeConfiguration<TNode> Include<TProperty>(params Expression<Func<TNode, TProperty>>[] properties)
    {
        properties.Select(x => ((MemberExpression)x.Body).Member.Name)
        .ToList()
        .ForEach(x =>
        {
            if (!IncludedProperties.Contains(x))
            {
                IncludedProperties = IncludedProperties.Append(x);
            }
        });
        return this;
    }
}