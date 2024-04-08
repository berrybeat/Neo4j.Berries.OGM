using System.Collections;
using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Match;
using Neo4j.Berries.OGM.Models.Sets;
using Neo4j.Berries.OGM.Utils;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Models.Queries;

public class NodeQuery
{
    internal List<IMatch> Matches { get; } = [];
    protected NodeConfiguration NodeConfig { get; }
    public DatabaseContext InternalDatabaseContext { get; }
    public string StartNodeLabel { get; }
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
        StartNodeLabel = startNodeLabel;
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
    where TResult : class
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
    where TResult : class
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
    where TResult : class
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
    where TResult : class
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

    #region Updates
    ///<summary>
    /// Updates the nodes found in the query
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    public void Update(Func<UpdateSet, UpdateSet> updateSetBuilder)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        InternalDatabaseContext.Run(cloneBuilder.ToString(), parameters);
    }
    ///<summary>
    /// Updates the nodes found in the query
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    public async Task UpdateAsync(Func<UpdateSet, UpdateSet> updateSetBuilder, CancellationToken cancellationToken = default)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        await InternalDatabaseContext.RunAsync(cloneBuilder.ToString(), parameters, cancellationToken);
    }
    ///<summary>
    /// Updates the nodes found in the query and returns the updated nodes
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    ///<returns>The updated node</returns>
    public IEnumerable<T> UpdateAndReturn<T>(Func<UpdateSet, UpdateSet> updateSetBuilder)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        ExpandCypherWithReturn(cloneBuilder);
        return InternalDatabaseContext.Run(
            cloneBuilder.ToString(),
            parameters,
            record => record.Convert<T>(Matches.First().StartNodeAlias)
        );
    }
    ///<summary>
    /// Updates the nodes found in the query and returns the updated nodes
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    ///<returns>The updated node</returns>
    public async Task<IEnumerable<T>> UpdateAndReturnAsync<T>(Func<UpdateSet, UpdateSet> updateSetBuilder, CancellationToken cancellationToken = default)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        ExpandCypherWithReturn(cloneBuilder);
        return await InternalDatabaseContext.RunAsync(
            cloneBuilder.ToString(),
            parameters,
            record => record.Convert<T>(Matches.First().StartNodeAlias),
            cancellationToken
        );
    }
    private Dictionary<string, object> PrepareUpdate(Func<UpdateSet, UpdateSet> updateSetBuilder, StringBuilder builder)
    {
        builder.AppendLine($"WITH {Matches.First().StartNodeAlias}");
        var updateSet = updateSetBuilder(new UpdateSet(builder, Matches.Count, Matches.First().StartNodeAlias));
        var parameters = new List<KeyValuePair<string, object>>();
        parameters.AddRange(QueryParameters.AsEnumerable());
        parameters.AddRange(updateSet.Parameters.AsEnumerable());
        return parameters.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
    private StringBuilder ExpandCypherWithReturn(StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine($"RETURN {Matches.First().StartNodeAlias}");
        return builder;
    }
    #endregion

    #region Connection
    ///<summary>
    /// Connects the nodes found in the query to another node
    ///</summary>
    ///<param name="property">The property name which has a relation defined for</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node</param>
    public void Connect(string property, Func<Eloquent, Eloquent> eloquentFunc)
    {
        var relationConfig = NodeConfig.Relations[property];
        var cloneBuilder = ConnectionBuilder(relationConfig, eloquentFunc);
        CreateRelation(relationConfig, cloneBuilder);
    }
    ///<summary>
    /// Connects the nodes found in the query to another node
    ///</summary>
    ///<param name="property">The property name which has a relation defined for</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node</param>
    public async Task ConnectAsync(string property, Func<Eloquent, Eloquent> eloquentFunc, CancellationToken cancellationToken = default)
    {
        var relationConfig = NodeConfig.Relations[property];
        var cloneBuilder = ConnectionBuilder(relationConfig, eloquentFunc);
        await CreateRelationAsync(relationConfig, cloneBuilder, cancellationToken);
    }

    private StringBuilder ConnectionBuilder(IRelationConfiguration relationConfiguration, Func<Eloquent, Eloquent> eloquentFunc)
    {
        var eloquent = eloquentFunc(new Eloquent(Matches.Count));
        var cloneBuilder = CypherBuilder.Clone();
        var match = new MatchModel(relationConfiguration.EndNodeLabel, eloquent, Matches.Count).ToCypher(cloneBuilder);
        Matches.Add(match);
        return cloneBuilder;
    }
    private async Task CreateRelationAsync(IRelationConfiguration relationConfig, StringBuilder builder, CancellationToken cancellationToken)
    {
        builder.BuildConnectionRelation(relationConfig, Matches);
        await InternalDatabaseContext.RunAsync(builder.ToString(), QueryParameters, cancellationToken);
        Matches.RemoveAt(Matches.Count - 1);
    }
    private void CreateRelation(IRelationConfiguration relationConfig, StringBuilder builder)
    {
        builder.BuildConnectionRelation(relationConfig, Matches);
        InternalDatabaseContext.Run(builder.ToString(), QueryParameters);
        Matches.RemoveAt(Matches.Count - 1);
    }
    #endregion

    #region Disconnect
        ///<summary>
    /// Removes the last relation given to the query between the root node and the relation's target node
    ///</summary>
    ///<exception cref="InvalidOperationException">Thrown when the last match is not a relation</exception>
    public void Disconnect()
    {
        var cloneBuilder = PrepareDisconnect();
        InternalDatabaseContext.Run(cloneBuilder.ToString(), QueryParameters);
    }

    ///<summary>
    /// Removes the last relation given to the query between the root node and the relation's target node
    ///</summary>
    ///<exception cref="InvalidOperationException">Thrown when the last match is not a relation</exception>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var cloneBuilder = PrepareDisconnect();
        await InternalDatabaseContext.RunAsync(cloneBuilder.ToString(), QueryParameters, cancellationToken);
    }

    private StringBuilder PrepareDisconnect()
    {
        var lastMatch = Matches.Last();
        if (string.IsNullOrEmpty(lastMatch.RelationAlias))
        {
            throw new InvalidOperationException("Cannot disconnect without a relation");
        }
        var cloneBuilder = CypherBuilder.Clone();
        cloneBuilder.AppendLine($"DELETE {lastMatch.RelationAlias}");
        return cloneBuilder;
    }
    #endregion

    #region Archive

    ///<summary>
    /// Archives the nodes matched with the query. The remove function will only set the ArchivedAt property to the node and will not HardDelete the node.<br>
    ///</summary>
    ///<returns>The archived nodes</returns>
    public IEnumerable<T> Archive<T>()
    {
        var key = Matches.First().StartNodeAlias;
        var archiveCypher = PrepareArchive().ToString();
        return ExecuteWithMap(record => record.Convert<T>(key), archiveCypher);
    }

    ///<summary>
    /// Archives the nodes matched with the query. The remove function will only set the ArchivedAt property to the node and will not HardDelete the node.<br>
    ///</summary>
    ///<returns>The archived nodes</returns>
    public async Task<IEnumerable<T>> ArchiveAsync<T>(CancellationToken cancellationToken = default)
    {
        var key = Matches.First().StartNodeAlias;
        var archiveCypher = PrepareArchive().ToString();
        return await ExecuteWithMapAsync(record => record.Convert<T>(key), archiveCypher, cancellationToken);
    }

    private StringBuilder PrepareArchive()
    {
        var key = Matches.First().StartNodeAlias;
        return CypherBuilder.AppendLines(
            $"SET {key}.ArchivedAt = timestamp()",
            $"WITH DISTINCT {key}",
            $"RETURN {key}"
        );
    }

    #endregion
}