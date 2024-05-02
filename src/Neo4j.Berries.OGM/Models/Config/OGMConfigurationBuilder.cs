using System.Reflection;

namespace Neo4j.Berries.OGM.Models.Config;

public class OGMConfigurationBuilder
{
    internal Assembly[] Assemblies { get; private set; } = [];
    internal bool _EnforceIdentifiers { get; private set; } = false;

    /// <summary>
    /// Reads the INodeConfiguration implementations from the given assemblies
    /// </summary>
    /// <param name="assemblies">The assemblies to read the configurations from</param>
    public OGMConfigurationBuilder ReadAssemblies(params Assembly[] assemblies)
    {
        Assemblies = assemblies;
        return this;
    }

    /// <summary>
    /// Enforces the identifiers for the nodes.
    /// </summary>
    /// <remarks>Either Create or Merge, the library will check if the passed node has an identifier configured for it and the identifier is passed.</remarks>
    public OGMConfigurationBuilder EnforceIdentifiers()
    {
        _EnforceIdentifiers = true;
        return this;
    }
}