using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Match;
using Neo4j.Berries.OGM.Utils;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Models.Queries;

public class NodeQuery
{
    internal List<IMatch> Matches { get; } = [];
    public NodeConfiguration NodeConfig { get; }
    public DatabaseContext InternalDatabaseContext { get; }
    private StringBuilder CypherBuilder { get; } = new StringBuilder();
    ///<summary>
    /// Returns the cypher query only for the matches. The return statement is not included
    ///</summary>
    public string Cypher => CypherBuilder.ToString();
    ///<summary>
    /// Returns the parameters used in the query
    ///</summary>
    public Dictionary<string, object> QueryParameters => Matches.SelectMany(match => match.GetParameters()).ToDictionary(pair => pair.Key, pair => pair.Value);
    public NodeQuery(string startNodeLabel, NodeConfiguration nodeConfiguration, Eloquent eloquent, DatabaseContext databaseContext)
    {
        var match = new MatchModel(startNodeLabel, eloquent, 0);
        Matches.Add(match);
        NodeConfig = nodeConfiguration;
        InternalDatabaseContext = databaseContext;
    }
    #region Match builder
    ///<summary>
    /// Adds a mandatory relation to the query. The property must be defined as a relation.
    ///</summary>
    ///<param name="property">The relation to include in the query</param>
    public NodeQuery WithRelation(string property)
    {
        return WithRelation(property, null);
    }
    ///<summary>
    /// Adds a mandatory relation to the query. The property must be defined as a relation.
    ///</summary>
    ///<param name="property">The relation to include in the query.</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node.</param>
    public NodeQuery WithRelation(string property, Func<Eloquent, Eloquent> eloquentFunc)
    {
        var relationConfig = NodeConfig.Relations[property];
        var eloquent = eloquentFunc(new Eloquent(Matches.Count));
        var match = new MatchRelationModel(
            relationConfig.EndNodeLabel,
            Matches.First(),
            relationConfig,
            eloquent,
            Matches.Count)
            .ToCypher(CypherBuilder);
        Matches.Add(match);
        return this;
    }
    #endregion

    #region Query executions
    ///<summary>
    /// Checks if the match has a existing path.
    ///</summary>
    ///<returns>True if a path exists, false otherwise</returns>
    public bool Any()
    {
        var _cypher = CypherBuilder.BuildAnyQuery(Matches).ToString();
        var response = InternalDatabaseContext
            .Run(_cypher, QueryParameters);
        return response.ElementAt(0).Get<bool>("any");
    }
    ///<summary>
    /// Checks if the match has a existing path.
    ///</summary>
    ///<returns>True if a path exists, false otherwise</returns>
    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        var _cypher = CypherBuilder.BuildAnyQuery(Matches).ToString();
        var response = await InternalDatabaseContext
            .RunAsync(_cypher, QueryParameters, cancellationToken);
        return response.ElementAt(0).Get<bool>("any");
    }
    ///<summary>
    /// Returns the count of the root node which the query is started with
    ///</summary>
    public int Count()
    {
        var _cypher = CypherBuilder.BuildCountQuery(Matches).ToString();
        var response = InternalDatabaseContext
            .Run(_cypher, QueryParameters);
        return response.ElementAt(0).Get<int>("count");
    }
    ///<summary>
    /// Returns the count of the root node which the query is started with
    ///</summary>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var _cypher = CypherBuilder.BuildCountQuery(Matches).ToString();
        var response = await InternalDatabaseContext
            .RunAsync(_cypher, QueryParameters, cancellationToken);
        return response.ElementAt(0).Get<int>("count");
    }
    ///<summary>
    /// Returns the first node found in the query. The node type is of type the node the query is started with
    ///</summary>
    public TResult FirstOrDefault<TResult>()
    where TResult: class
    {
        var key = Matches.First().StartNodeAlias;
        var _cypher = CypherBuilder.BuildFirstOrDefaultQuery(Matches).ToString();
        var response = ExecuteWithMap(record => record.Convert<TResult>(key), _cypher);
        if (!response.Any()) return null;
        return response.ElementAt(0);
    }
    ///<summary>
    /// Returns the first node found in the query. The node type is of type the node the query is started with
    ///</summary>
    public async Task<TResult> FirstOrDefaultAsync<TResult>(CancellationToken cancellationToken = default)
    where TResult: class
    {
        var key = Matches.First().StartNodeAlias;
        var _cypher = CypherBuilder.BuildFirstOrDefaultQuery(Matches).ToString();
        var response = await ExecuteWithMapAsync(record => record.Convert<TResult>(key), _cypher, cancellationToken);
        if (!response.Any()) return null;
        return response.ElementAt(0);
    }
    ///<summary>
    /// Returns a list of nodes found in the query. The node type is of type the node the query is started with
    ///</summary>
    public List<TResult> ToList<TResult>()
    where TResult: class
    {
        var key = Matches.First().StartNodeAlias;
        var _cypher = CypherBuilder.BuildListQuery(Matches).ToString();
        var response = ExecuteWithMap(record => record.Convert<TResult>(key), _cypher);
        return [.. response];
    }
    ///<summary>
    /// Returns a list of nodes found in the query. The node type is of type the node the query is started with
    ///</summary>
    public async Task<List<TResult>> ToListAsync<TResult>(CancellationToken cancellationToken = default)
    where TResult: class
    {
        var key = Matches.First().StartNodeAlias;
        var _cypher = CypherBuilder.BuildListQuery(Matches).ToString();
        var response = await ExecuteWithMapAsync(record => record.Convert<TResult>(key), _cypher, cancellationToken);
        return [.. response];
    }
    private async Task<IEnumerable<TResult>> ExecuteWithMapAsync<TResult>(Func<IRecord, TResult> mapper, string cypher, CancellationToken cancellationToken = default)
    {
        return await InternalDatabaseContext.RunAsync(cypher, QueryParameters, mapper, cancellationToken);
    }
    private IEnumerable<TResult> ExecuteWithMap<TResult>(Func<IRecord, TResult> mapper, string cypher, CancellationToken cancellationToken = default)
    {
        return InternalDatabaseContext.Run(cypher, QueryParameters, mapper);
    }
    #endregion
}