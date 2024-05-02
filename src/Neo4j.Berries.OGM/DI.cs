using System.Reflection;
using Neo4j.Berries.OGM.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM;

public static class DI
{
    /// <summary>
    /// Adds the Neo4j context to the service collection
    /// </summary>
    [Obsolete("This method is obsolete, please use the other AddNeo4j<TContext> method override instead.")]
    public static IServiceCollection AddNeo4j<TContext>(this IServiceCollection services, IConfiguration configuration, params Assembly[] assembly)
    where TContext : GraphContext
    {
        services.AddSingleton(sp =>
        {
            return new Neo4jSingletonContext(assembly);
        });
        services.AddScoped(sp =>
        {
            sp.GetRequiredService<Neo4jSingletonContext>();
            var neo4jOptions = new Neo4jOptions(configuration);
            return Activator.CreateInstance(typeof(TContext), neo4jOptions) as TContext;
        });
        return services;
    }

    public static IServiceCollection AddNeo4j<TContext>(this IServiceCollection services, IConfiguration configuration, Action<OGMConfigurationBuilder> options)
    where TContext : GraphContext
    {
        var configurationBuilder = new OGMConfigurationBuilder();
        options(configurationBuilder);
        services.AddSingleton(sp =>
        {
            return new Neo4jSingletonContext(configurationBuilder);
        });
        services.AddScoped(sp =>
        {
            sp.GetRequiredService<Neo4jSingletonContext>();
            var neo4jOptions = new Neo4jOptions(configuration);
            return Activator.CreateInstance(typeof(TContext), neo4jOptions) as TContext;
        });
        return services;
    }
}