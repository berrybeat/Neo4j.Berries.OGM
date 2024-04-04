using System.Linq.Expressions;
using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

public class UpdateSet<TNode>
where TNode : class
{
    public string NodeAlias { get; }

    private readonly StringBuilder CypherBuilder;

    internal int Index { get; }
    internal NodeConfiguration NodeConfig { get; } = new NodeConfiguration();
    internal Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
    internal string CurrentParameterName => $"up_{Index}_{Parameters.Count}";
    public UpdateSet(StringBuilder cypherBuilder, int index, string nodeAlias)
    {
        cypherBuilder.Append("SET").Append(' ', 1);
        NodeAlias = nodeAlias;
        CypherBuilder = cypherBuilder;
        Index = index;
        if(Neo4jSingletonContext.Configs.TryGetValue(typeof(TNode).Name, out NodeConfiguration value))
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
        var property = ((MemberExpression)expression.Body).Member.Name;
        var parameterName = CurrentParameterName;
        if (Parameters.Count > 0)
            CypherBuilder.Append($", {NodeAlias}.{property} = ${parameterName}");
        else
            CypherBuilder.Append($"{NodeAlias}.{property} = ${parameterName}");
        Parameters.Add(parameterName, value.ToNeo4jValue());
        return this;
    }
    /// <summary>
    /// Sets a custom property of the node to the given value
    /// </summary>
    /// <param name="property">The property to set</param>
    /// <param name="value">The value to set the property to</param>
    public UpdateSet<TNode> Set<TProperty>(string property, TProperty value)
    {
        var parameterName = CurrentParameterName;
        if (Parameters.Count > 0)
            CypherBuilder.Append($", {NodeAlias}.{property} = ${parameterName}");
        else
            CypherBuilder.Append($"{NodeAlias}.{property} = ${parameterName}");
        Parameters.Add(parameterName, value.ToNeo4jValue());
        return this;
    }
    /// <summary>
    /// Sets the properties of the node to the given values
    /// </summary>
    /// <param name="node">An instance with the new values of the node which needs to be updated.</param>
    public UpdateSet<TNode> Set(TNode node)
    {
        var properties = typeof(TNode).GetProperties()
            .Where(x =>
                (!NodeConfig.ExcludedProperties.Contains(x.Name) && !NodeConfig.ExcludedProperties.IsEmpty) ||
                (NodeConfig.IncludedProperties.Contains(x.Name) && !NodeConfig.IncludedProperties.IsEmpty) ||
                (NodeConfig.ExcludedProperties.IsEmpty && NodeConfig.IncludedProperties.IsEmpty)
            );
        foreach(var prop in properties) {
            var value = prop.GetValue(node);
            var parameterName = CurrentParameterName;
            if (Parameters.Count > 0)
                CypherBuilder.Append($", {NodeAlias}.{prop.Name} = ${parameterName}");
            else
                CypherBuilder.Append($"{NodeAlias}.{prop.Name} = ${parameterName}");
                
            Parameters.Add(parameterName, value.ToNeo4jValue());
        }
        return this;
    }
}