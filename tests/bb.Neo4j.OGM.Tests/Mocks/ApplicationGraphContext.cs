using berrybeat.Neo4j.OGM.Contexts;
using berrybeat.Neo4j.OGM.Models;
using berrybeat.Neo4j.OGM.Tests.Mocks.Models;
using Neo4j.Driver;

namespace berrybeat.Neo4j.OGM.Tests.Mocks;

public class ApplicationGraphContext(Neo4jOptions neo4jOptions) : GraphContext(neo4jOptions)
{
    public NodeSet<Movie> Movies { get; private set; }
    public NodeSet<Person> People { get; private set; }
    //This should not be initialized
    public List<Movie> SilentMovies { get; set; }
}