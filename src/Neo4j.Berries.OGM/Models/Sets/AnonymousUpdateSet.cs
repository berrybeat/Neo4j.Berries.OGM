using System.Text;
using Neo4j.Berries.OGM.Helpers;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

public class UpdateSet
{
    public string NodeAlias { get; }
    private readonly StringBuilder CypherBuilder;
    internal int Index { get; }
    internal NodeConfiguration NodeConfig { get; set; } = new NodeConfiguration();
    internal Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
    internal string CurrentParameterName => $"up_{Index}_{Parameters.Count}";
    public UpdateSet(StringBuilder cypherBuilder, int index, string nodeAlias, NodeConfiguration nodeConfig = null)
    {
        cypherBuilder.Append("SET").Append(' ', 1);
        NodeAlias = nodeAlias;
        CypherBuilder = cypherBuilder;
        Index = index;
        NodeConfig = nodeConfig ?? NodeConfig;
    }

    /// <summary>
    /// Sets a property of the node to the given value
    /// </summary>
    /// <param name="expression">The property to set</param>
    /// <param name="value">The value to set the property to</param>
    public UpdateSet Set(string property, object value)
    {
        var parameterName = CurrentParameterName;
        if (Parameters.Count > 0)
            CypherBuilder.Append($", {NodeAlias}.{property} = ${parameterName}");
        else
            CypherBuilder.Append($"{NodeAlias}.{property} = ${parameterName}");
        Parameters.Add(parameterName, value.ToNeo4jValue());
        return this;
    }

    public UpdateSet Set(Dictionary<string, object> node)
    {
        return SetAll(node);
    }

    /// <summary>
    /// Sets the properties of the node to the given values
    /// </summary>
    /// <param name="node">An instance with the new values of the node which needs to be updated.</param>
    protected UpdateSet SetAll(object node)
    {
        var propertiesHelper = new PropertiesHelper(node);
        var validProperties = propertiesHelper.GetValidProperties(NodeConfig);
        foreach (var prop in validProperties)
        {
            var parameterName = CurrentParameterName;
            if (Parameters.Count > 0)
                CypherBuilder.Append($", {NodeAlias}.{prop.Key} = ${parameterName}");
            else
                CypherBuilder.Append($"{NodeAlias}.{prop.Key} = ${parameterName}");

            Parameters.Add(parameterName, prop.Value.ToNeo4jValue());
        }
        return this;
    }
}