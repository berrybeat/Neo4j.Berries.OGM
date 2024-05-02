using MovieGraph.Database.Models;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Sets;
namespace MovieGraph.Database;

public class ApplicationGraphContext(Neo4jOptions options) : GraphContext(options)
{
    public NodeSet<Movie> Movies { get; private set; }
    public NodeSet<Person> People { get; private set; }
}