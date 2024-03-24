using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models;
using Neo4j.Berries.OGM.Tests.Mocks.Models;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Tests.Mocks;

public class ApplicationGraphContext(Neo4jOptions neo4jOptions) : GraphContext(neo4jOptions)
{
    public NodeSet<Movie> Movies { get; private set; }
    public NodeSet<Person> People { get; private set; }
    //This should not be initialized
    public List<Movie> SilentMovies { get; set; }
}