using System.Reflection;
using Neo4j.Berries.OGM.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Neo4j.Berries.OGM;

public static class DI
{
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
}