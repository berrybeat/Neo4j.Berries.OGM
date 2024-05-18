using System.Linq.Expressions;
using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Match;
using Neo4j.Berries.OGM.Models.Sets;
using Neo4j.Berries.OGM.Utils;
namespace Neo4j.Berries.OGM.Models.Queries;

public class NodeQuery<TNode> : NodeQuery
where TNode : class
{

    public NodeQuery(Eloquent<TNode> eloquent, DatabaseContext databaseContext) : base(typeof(TNode).Name, eloquent, databaseContext)
    {
        if (Neo4jSingletonContext.Configs.TryGetValue(typeof(TNode).Name, out NodeConfiguration value))
        {
            NodeConfig = value;
        }
    }
    #region Match builder
    ///<summary>
    /// Adds a mandatory relation to the query. The property must be defined as a relation.
    ///</summary>
    ///<param name="expression">The relation to include in the query</param>
    public NodeQuery<TNode> WithRelation<TProperty>(Expression<Func<TNode, TProperty>> expression)
    where TProperty : class
    {
        WithRelation(expression.GetPropertyName());
        return this;
    }
    ///<summary>
    /// Adds a mandatory relation to the query. The property must be defined as a relation.
    ///</summary>
    ///<param name="expression">The relation to include in the query.</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node. IGNORE intellicense here when searching through a collection of relations</param>
    public NodeQuery<TNode> WithRelation<TProperty>(Expression<Func<TNode, ICollection<TProperty>>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc)
    where TProperty : class
    {
        var eloquent = eloquentFunc(new Eloquent<TProperty>(Matches.Count));
        WithRelation(expression.GetPropertyName(), eloquent);
        return this;
    }
    ///<summary>
    /// Adds a mandatory relation to the query. The property must be defined as a relation.
    ///</summary>
    ///<param name="expression">The relation to include in the query.</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node. IGNORE intellisense here when searching through a collection of relations</param>
    public NodeQuery<TNode> WithRelation<TProperty>(Expression<Func<TNode, TProperty>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc)
    where TProperty : class
    {
        Eloquent<TProperty> eloquent = null;
        if (eloquentFunc != null)
        {
            eloquent = eloquentFunc(new Eloquent<TProperty>(Matches.Count));
        }
        WithRelation(expression.GetPropertyName(), eloquent);
        return this;
    }
    #endregion

    #region Query executions
    ///<summary>
    /// Returns the first node found in the query. The node type is of type the node the query is started with
    ///</summary>
    public TNode FirstOrDefault()
    {
        return FirstOrDefault<TNode>();
    }
    ///<summary>
    /// Returns the first node found in the query. The node type is of type the node the query is started with
    ///</summary>
    public async Task<TNode> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync<TNode>(cancellationToken);
    }
    ///<summary>
    /// Returns a list of nodes found in the query. The node type is of type the node the query is started with
    ///</summary>
    public List<TNode> ToList()
    {
        return ToList<TNode>();
    }
    ///<summary>
    /// Returns a list of nodes found in the query. The node type is of type the node the query is started with
    ///</summary>
    public async Task<List<TNode>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return await ToListAsync<TNode>(cancellationToken);
    }
    #endregion

    #region Updates
    ///<summary>
    /// Updates the nodes found in the query
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    public void Update(Func<UpdateSet<TNode>, UpdateSet<TNode>> updateSetBuilder)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        InternalDatabaseContext.Run(cloneBuilder.ToString(), parameters);
    }
    ///<summary>
    /// Updates the nodes found in the query
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    public async Task UpdateAsync(Func<UpdateSet<TNode>, UpdateSet<TNode>> updateSetBuilder, CancellationToken cancellationToken = default)
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
    public IEnumerable<TNode> UpdateAndReturn(Func<UpdateSet<TNode>, UpdateSet<TNode>> updateSetBuilder)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        ExpandCypherWithReturn(cloneBuilder);
        return InternalDatabaseContext.Run(
            cloneBuilder.ToString(),
            parameters,
            record => record.Convert<TNode>(Matches.First().StartNodeAlias)
        );
    }
    ///<summary>
    /// Updates the nodes found in the query and returns the updated nodes
    ///</summary>
    ///<param name="updateSetBuilder">A function to build the update set</param>
    ///<returns>The updated node</returns>
    public async Task<IEnumerable<TNode>> UpdateAndReturnAsync(Func<UpdateSet<TNode>, UpdateSet<TNode>> updateSetBuilder, CancellationToken cancellationToken = default)
    {
        var cloneBuilder = CypherBuilder.Clone();
        var parameters = PrepareUpdate(updateSetBuilder, cloneBuilder);
        ExpandCypherWithReturn(cloneBuilder);
        return await InternalDatabaseContext.RunAsync(
            cloneBuilder.ToString(),
            parameters,
            record => record.Convert<TNode>(Matches.First().StartNodeAlias),
            cancellationToken
        );
    }
    private Dictionary<string, object> PrepareUpdate(Func<UpdateSet<TNode>, UpdateSet<TNode>> updateSetBuilder, StringBuilder builder)
    {
        var updateSet = updateSetBuilder(new UpdateSet<TNode>(builder, Matches.Count, Matches.First().StartNodeAlias));
        return PrepareUpdate(updateSet, builder);
    }
    #endregion

    #region Connection
    ///<summary>
    /// Connects the nodes found in the query to another node
    ///</summary>
    ///<param name="expression">The relation to connect the nodes with</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node</param>
    public void Connect<TProperty>(Expression<Func<TNode, ICollection<TProperty>>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc)
    where TProperty : class
    {
        var cloneBuilder = ConnectionBuilder(expression, eloquentFunc);
        CreateRelation(expression, cloneBuilder);
    }
    ///<summary>
    /// Connects the nodes found in the query to another node
    ///</summary>
    ///<param name="expression">The relation to connect the nodes with</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node</param>
    public async Task ConnectAsync<TProperty>(Expression<Func<TNode, ICollection<TProperty>>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc, CancellationToken cancellationToken = default)
    where TProperty : class
    {
        var cloneBuilder = ConnectionBuilder(expression, eloquentFunc);
        await CreateRelationAsync(expression, cloneBuilder, cancellationToken);
    }

    private StringBuilder ConnectionBuilder<TProperty>(Expression<Func<TNode, ICollection<TProperty>>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc)
    where TProperty : class
    {
        var relationConfig = NodeConfig.Relations[expression.GetPropertyName()];
        var eloquent = eloquentFunc(new Eloquent<TProperty>(Matches.Count));
        return ConnectionBuilder(relationConfig, eloquent);
    }

    ///<summary>
    /// Connects the nodes found in the query to another node
    ///</summary>
    ///<param name="expression">The relation to connect the nodes with</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node</param>
    public async Task ConnectAsync<TProperty>(Expression<Func<TNode, TProperty>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc, CancellationToken cancellationToken = default)
    where TProperty : class
    {
        var eloquent = eloquentFunc(new Eloquent<TProperty>(Matches.Count));
        var cloneBuilder = CypherBuilder.Clone();
        var match = new MatchModel<TProperty>(eloquent, Matches.Count).ToCypher(cloneBuilder);
        Matches.Add(match);
        await CreateRelationAsync(expression, cloneBuilder, cancellationToken);
    }
    ///<summary>
    /// Connects the nodes found in the query to another node
    ///</summary>
    ///<param name="expression">The relation to connect the nodes with</param>
    ///<param name="eloquentFunc">A function to build the eloquent query for the relation's target node</param>
    public void Connect<TProperty>(Expression<Func<TNode, TProperty>> expression, Func<Eloquent<TProperty>, Eloquent<TProperty>> eloquentFunc)
    where TProperty : class
    {
        var eloquent = eloquentFunc(new Eloquent<TProperty>(Matches.Count));
        var cloneBuilder = CypherBuilder.Clone();
        var match = new MatchModel<TProperty>(eloquent, Matches.Count).ToCypher(cloneBuilder);
        Matches.Add(match);
        CreateRelation(expression, cloneBuilder);
    }
    private async Task CreateRelationAsync<TProperty>(Expression<Func<TNode, TProperty>> expression, StringBuilder builder, CancellationToken cancellationToken)
    where TProperty : class
    {
        var relationConfig = NodeConfig.Relations[((MemberExpression)expression.Body).Member.Name];
        await CreateRelationAsync(relationConfig, builder, cancellationToken);
    }
    private void CreateRelation<TProperty>(Expression<Func<TNode, TProperty>> expression, StringBuilder builder)
    where TProperty : class
    {
        var relationConfig = NodeConfig.Relations[((MemberExpression)expression.Body).Member.Name];
        CreateRelation(relationConfig, builder);
    }
    #endregion

    #region Archive

    ///<summary>
    /// Archives the nodes matched with the query. The remove function will only set the ArchivedAt property to the node and will not HardDelete the node.<br>
    ///</summary>
    ///<returns>The archived nodes</returns>
    public IEnumerable<TNode> Archive()
    {
        return Archive<TNode>();
    }

    ///<summary>
    /// Archives the nodes matched with the query. The remove function will only set the ArchivedAt property to the node and will not HardDelete the node.<br>
    ///</summary>
    ///<returns>The archived nodes</returns>
    public async Task<IEnumerable<TNode>> ArchiveAsync(CancellationToken cancellationToken = default)
    {
        return await ArchiveAsync<TNode>(cancellationToken);
    }
    #endregion
}