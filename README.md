# Neo4j berries OGM

This repository is home to berrybeat's dotnet OGM (Object-Graph-Mapping) library for neo4j. It supports basic queries, create, update, connect/disconnect relations.

## Version

* Latest version: v1.0.0-preview-1
* Under development: v1.0.0-preview-1
* Dev version: v1.0.0-preview-1


## Installation
This library is available in nuget and can be installed with the following command:

```
dotnet add package Neo4j.Berries.OGM
```

**Note:** This is a dotnet 8 specific library.

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
        "Database": "<neo4j-database-name>"
    }
}
```

* Neo4j.Url: This is the url to neo4j. For example: `http://localhost:7687`
* Neo4j.Username: The username for your database
* Neo4j.Password: The password for your database
* Neo4j.Database: This is optional. If not passed, it will connect to the default database. If passed, it will connect to the passed database.

## Contributing

We welcome community enhancements and bug fixed. If you want to contribute to this repository please check [How to contribute](./.github/CONTRIBUTING.md) for more information

## Resources

For more information about the library check [Getting Started](#)

For an example please check [Example/MovieGraph](./example/MovieGraph/)

For checking the Neo4j Driver's manual please check [The Neo4j .NET Driver Manual v5.18](https://neo4j.com/docs/dotnet-manual/current/)