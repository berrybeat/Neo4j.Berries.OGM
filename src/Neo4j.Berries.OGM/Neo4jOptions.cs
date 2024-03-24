using Microsoft.Extensions.Configuration;
using Neo4j.Driver;

public class Neo4jOptions
{
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }
    internal IDriver Driver { get; set; }
    public Neo4jOptions()
    {
        InitDriver();
    }
    public Neo4jOptions(IConfiguration configuration)
    {
        Url = configuration["Neo4j:Url"];
        Username = configuration["Neo4j:Username"];
        Password = configuration["Neo4j:Password"];
        Database = configuration["Neo4j:Database"];
        InitDriver();
    }

    private void InitDriver()
    {
        Driver = GraphDatabase.Driver(Url, AuthTokens.Basic(Username, Password));
    }
}