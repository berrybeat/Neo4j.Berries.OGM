using System.Linq.Expressions;
using Neo4j.Berries.OGM.Contexts;

namespace Neo4j.Berries.OGM.Models.Config;

public class MergeConfiguration<TNode>
where TNode : class
{
    internal IEnumerable<string> IncludedProperties { get; private set; } = [];

    public MergeConfiguration<TNode> Include<TProperty>(params Expression<Func<TNode, TProperty>>[] properties)
    {
        var mergeProperties = properties.Select(x => Neo4jSingletonContext.PropertyCaseConverter(((MemberExpression)x.Body).Member.Name));
        foreach (var prop in mergeProperties)
        {
            if (!IncludedProperties.Contains(prop))
            {
                IncludedProperties = IncludedProperties.Append(prop);
            }
        }
        return this;
    }
}