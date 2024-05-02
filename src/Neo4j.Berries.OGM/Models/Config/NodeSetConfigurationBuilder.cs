namespace Neo4j.Berries.OGM.Models.Config;

public class NodeSetConfigurationBuilder
{
    internal Dictionary<string, NodeConfiguration> NodeSetConfigurations;

    /// <summary>
    /// Gives the option to configure a specific node set.
    /// </summary>
    /// <param name="label">The label of the node for the NodeSet</param>
    /// <param name="options">The options to configure the node set</param>
    public NodeSetConfigurationBuilder For(string label, Action<NodeConfigurationBuilder> options)
    {
        var nodeConfigurationBuilder = new NodeConfigurationBuilder();
        options(nodeConfigurationBuilder);
        NodeSetConfigurations.Add(label, nodeConfigurationBuilder.NodeConfiguration);
        return this;
    }
}