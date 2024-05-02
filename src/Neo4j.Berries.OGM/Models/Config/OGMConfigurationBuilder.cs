using System.Reflection;

namespace Neo4j.Berries.OGM.Models.Config;

public class OGMConfigurationBuilder(IServiceProvider serviceProvider)
{
    internal Assembly[] Assemblies { get; private set; } = [];
    internal Dictionary<string, NodeConfiguration> NodeSetConfigurations { get; private set; } = [];
    public IServiceProvider ServiceProvider { get; } = serviceProvider;


    /// <summary>
    /// Reads the INodeConfiguration implementations from the given assemblies
    /// </summary>
    /// <param name="assemblies">The assemblies to read the configurations from</param>
    public OGMConfigurationBuilder ConfigureFromAssemblies(params Assembly[] assemblies)
    {
        Assemblies = assemblies;
        return this;
    }

    /// <summary>
    /// Configures the node sets, by passing the label and the configuration options directly.
    /// </summary>
    /// <remarks>Use this method if you want to configure the node sets directly. IMPORTANT: This Configuration will overwrite the Assemblies configuration if they overlap.</remarks>
    public OGMConfigurationBuilder Configure(Action<NodeSetConfigurationBuilder> builder)
    {
        var nodeSetConfigurationBuilder = new NodeSetConfigurationBuilder();
        builder(nodeSetConfigurationBuilder);
        NodeSetConfigurations = nodeSetConfigurationBuilder.NodeSetConfigurations;
        return this;
    }

    /// <summary>
    /// Enforces the identifiers for the nodes.
    /// </summary>
    /// <remarks>Either Create or Merge, the library will check if the passed node has an identifier configured for it and the identifier is passed.</remarks>
    public bool EnforceIdentifiers { get; set; } = false;
}