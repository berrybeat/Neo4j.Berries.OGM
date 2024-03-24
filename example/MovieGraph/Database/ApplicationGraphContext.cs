using berrybeat.Neo4j.OGM.Contexts;
using berrybeat.Neo4j.OGM.Models;
using MovieGraph.Database.Models;
using Neo4j.Driver;

namespace MovieGraph.Database;

public class ApplicationGraphContext(IDriver driver) : GraphContext(driver)
{
    public NodeSet<Movie> Movies { get; private set; }
    public NodeSet<Person> People { get; private set; }
}