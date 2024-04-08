using System.Linq.Expressions;
using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

public class UpdateSet<TNode> : UpdateSet
where TNode : class
{
    public UpdateSet(StringBuilder cypherBuilder, int index, string nodeAlias) : base(cypherBuilder, index, nodeAlias)
    {
        if (Neo4jSingletonContext.Configs.TryGetValue(typeof(TNode).Name, out NodeConfiguration value))
        {
            NodeConfig = value;
        }
    }
    /// <summary>
    /// Sets a property of the node to the given value
    /// </summary>
    /// <param name="expression">The property to set</param>
    /// <param name="value">The value to set the property to</param>
    public UpdateSet<TNode> Set<TProperty>(Expression<Func<TNode, TProperty>> expression, TProperty value)
    {
        var property = expression.GetPropertyName();
        base.Set(property, value);
        return this;

    }
    /// <summary>
    /// Sets a custom property of the node to the given value
    /// </summary>
    /// <param name="property">The property to set</param>
    /// <param name="value">The value to set the property to</param>
    public UpdateSet<TNode> Set<TProperty>(string property, TProperty value)
    {
        base.Set(property, value);
        return this;
    }
    /// <summary>
    /// Sets the properties of the node to the given values
    /// </summary>
    /// <param name="node">An instance with the new values of the node which needs to be updated.</param>
    public UpdateSet<TNode> Set(TNode node)
    {
        base.SetAll(node);
        return this;
    }
}