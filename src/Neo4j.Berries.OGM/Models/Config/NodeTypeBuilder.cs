using System.Linq.Expressions;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Models.Config;

public class NodeTypeBuilder<TNode>
where TNode : class
{
    private static string TypeName => typeof(TNode).Name;
    private NodeConfiguration Config
    {
        get
        {
            if (!Neo4jSingletonContext.Configs.TryGetValue(TypeName, out NodeConfiguration value))
            {
                value = new NodeConfiguration();
                Neo4jSingletonContext.Configs[TypeName] = value;
            }
            return value;
        }
    }

    /// <summary>
    /// The property will be used to create a relation with other nodes
    /// </summary>
    public RelationConfiguration<TNode, TProperty> HasRelationWithMultiple<TProperty>(Expression<Func<TNode, IEnumerable<TProperty>>> expression, string label, RelationDirection direction)
    where TProperty : class
    {
        var propertyName = ((MemberExpression)expression.Body).Member.Name;
        var relationConfig = new RelationConfiguration<TNode, TProperty>(label, direction);
        Config.Relations[propertyName] = relationConfig;
        Exclude(expression);
        return relationConfig;
    }
    /// <summary>
    /// The property will be used to create a relation with other nodes
    /// </summary>
    public RelationConfiguration<TNode, TRelation, TProperty> HasRelationWithMultiple<TRelation, TProperty>(Expression<Func<TNode, Relations<TRelation, TProperty>>> expression, string label, RelationDirection direction)
    where TRelation : class
    where TProperty : class
    {
        var propertyName = ((MemberExpression)expression.Body).Member.Name;
        var relationConfig = new RelationConfiguration<TNode, TRelation, TProperty>(label, direction);
        Config.Relations[propertyName] = relationConfig;
        Exclude(expression);
        return relationConfig;
    }
    /// <summary>
    /// The property will be used to create a relation with another node
    /// </summary>
    public RelationConfiguration<TNode, TProperty> HasRelationWithSingle<TProperty>(Expression<Func<TNode, TProperty>> expression, string label, RelationDirection direction)
    where TProperty : class
    {
        var propertyName = ((MemberExpression)expression.Body).Member.Name;
        var relationConfig = new RelationConfiguration<TNode, TProperty>(label, direction);
        Config.Relations[propertyName] = relationConfig;
        Exclude(expression);
        return relationConfig;
    }
    /// <summary>
    /// The property will be included in the node. If exclude is used, include will be ignored
    /// </summary>
    public void Include<TProperty>(params Expression<Func<TNode, TProperty>>[] expressions)
    {
        expressions
                .Select(x => ((MemberExpression)x.Body).Member.Name).ToList()
                .ForEach(Config.IncludedProperties.Add);
    }

    /// <summary>
    /// The property will be excluded from the node. If this is used the include cannot be used
    /// </summary>
    public void Exclude<TProperty>(params Expression<Func<TNode, TProperty>>[] expressions)
    {
        expressions
                .Select(x => ((MemberExpression)x.Body).Member.Name).ToList()
                .ForEach(x =>
                {
                    if (!Config.ExcludedProperties.Contains(x))
                    {
                        Config.ExcludedProperties.Add(x);
                    }
                });
    }
}