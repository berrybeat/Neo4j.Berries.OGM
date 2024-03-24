# Neo4j OGM

This repository is home to berrybeat's dotnet OGM library for neo4j. It supports basic queries, create, update, connect/disconnect relations.

### Versions

* Latest version: v0.3.0
* Under development: v0.4.0
* Dev version: v0.4.0-preview-1


## Installation
This library is available in nuget and can be installed with the following command:
```
dotnet add package berrybeat.Neo4j.OGM
```

## Basic Usage
The following snippet demonstrates the idea of how this library will be used with dependency injection.
```csharp
public class MoviesController(ApplicationGraphContext graphContext): ControllerBase 
{
    [HttpPost]
    public async Task Create(Movie request) 
    {
        graphContext.Movies.Add(request);
        await graphContext.SaveChangesAsync();
    }
    [HttpPost]
    public async Task Update(Movie request) 
    {
        await graphContext.Movies
            .Match(x => x.Where(y => y.Id, request.Id))
            .UpdateAsync(x => x.Set(request)) //Sets the whole object
    }
}
```
The queries as you see are not supported with LINQ and instead you need to use the designed Eloquent. Also to use the library you need to add the following to your `appsettings.json`.
```json
{
    "Neo4j": {
        "Url": "<neo4j-address>",
        "Username": "<neo4j-user>",
        "Password": "<neo4j-password>",
        "Database": "<neo4j-database-name>" //when not set, the default database will be used!
    }
}
```


## Resources

For more information about the library check [Getting Started](https://gitlab.berrybeat.de/nuget/berrybeat.neo4j.ogm/-/wikis/Getting-Started)

For an example please check [Example/MovieGraph](./example/MovieGraph/)

For checking the Neo4j Driver's manual please check [The Neo4j .NET Driver Manual v5.18](https://neo4j.com/docs/dotnet-manual/current/)