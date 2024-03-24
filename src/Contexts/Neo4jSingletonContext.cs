using System.Reflection;
using berrybeat.Neo4j.OGM.Interfaces;
using berrybeat.Neo4j.OGM.Models;
using berrybeat.Neo4j.OGM.Models.Config;

namespace berrybeat.Neo4j.OGM.Contexts;

internal class Neo4jSingletonContext
{
    private readonly Assembly[] _assemblies;

    internal static Dictionary<string, NodeConfiguration> Configs { get; private set; } = [];
    public Neo4jSingletonContext(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
        ParseAssemblyForConfigurations();
    }
    private void ParseAssemblyForConfigurations()
    {
        var interfaceType = typeof(INodeConfiguration<>);
        var configTypes = _assemblies.SelectMany(x => x.GetTypes())
            .Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == interfaceType))
            .Select(x => new
            {
                Interface = x.GetInterfaces().First(y => y.IsGenericType && y.GetGenericTypeDefinition() == interfaceType),
                Instance = x,
                x.Name
            });
        foreach (var configType in configTypes)
        {
            var genericArguments = configType.Interface.GetGenericArguments();
            if (genericArguments.Length > 1 || genericArguments.Length == 0)
            {
                throw new InvalidOperationException($"Invalid number of generic arguments on {configType.Name}");
            }

            var builder = Activator.CreateInstance(typeof(NodeTypeBuilder<>).MakeGenericType(genericArguments[0]));
            var config = Activator.CreateInstance(configType.Instance);
            var configureMethod = configType.Instance.GetMethod(nameof(INodeConfiguration<object>.Configure));
            configureMethod.Invoke(config, [builder]);           
        }

    }
}