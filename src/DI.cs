using System.Reflection;
using berrybeat.Neo4j.OGM.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace berrybeat.Neo4j.OGM;

public static class DI
{
    public static IServiceCollection AddNeo4j<TContext>(this IServiceCollection services, IConfiguration configuration, params Assembly[] assembly)
    where TContext : GraphContext
    {
        services.Configure<Neo4jOptions>(configuration.GetSection("Neo4j"));
        services.AddSingleton(sp =>
        {
            return new Neo4jSingletonContext(assembly);
        });
        services.AddScoped(sp =>
        {
            sp.GetRequiredService<Neo4jSingletonContext>();
            var options = sp.GetRequiredService<IOptions<Neo4jOptions>>().Value;
            return Activator.CreateInstance(typeof(TContext), options) as TContext;
        });
        return services;
    }
}