using System.Reflection;
using System.Text;
using berrybeat.Neo4j.OGM.Interfaces;

namespace berrybeat.Neo4j.OGM.Contexts;

public abstract class GraphContext
{
    public DatabaseContext Database { get; private set; }
    internal StringBuilder CypherBuilder { get; } = new StringBuilder();
    private IEnumerable<PropertyInfo> NodeSets { get; set; }
    public GraphContext(Neo4jOptions options)
    {
        Database = new DatabaseContext(options);
        InitDbSets();
    }

    private void InitDbSets()
    {
        NodeSets = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x =>
                x.PropertyType.IsAssignableTo(typeof(INodeSet)));
        for (var i = 0; i < NodeSets.Count(); i++)
        {
            var nodeSet = NodeSets.ElementAt(i);
            var instance = Activator.CreateInstance(nodeSet.PropertyType, i, Database, CypherBuilder);
            nodeSet.SetValue(this, instance);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<KeyValuePair<string, object>> parameters = [];
        GetCreateParameters(parameters);

        await Database.RunAsync(CypherBuilder.ToString(), parameters, cancellationToken);

        //This will prevent the SaveChangesAsync to save multiple times accidentally.
        ResetCreateCommands();
        CypherBuilder.Clear();
    }

    public void SaveChanges()
    {
        List<KeyValuePair<string, object>> parameters = [];
        GetCreateParameters(parameters);

        Database.Run(CypherBuilder.ToString(), parameters);

        //This will prevent the SaveChangesAsync to save multiple times accidentally.
        ResetCreateCommands();
        CypherBuilder.Clear();
    }

    private void GetCreateParameters(List<KeyValuePair<string, object>> parameters)
    {
        foreach (var prop in NodeSets)
        {
            var nodeSet = (INodeSet)prop.GetValue(this);
            var nodeSetParams = nodeSet.CreateCommands.SelectMany(createCommand => createCommand.Parameters);
            parameters.AddRange(nodeSetParams);
        }
    }
    private void ResetCreateCommands()
    {
        foreach (var prop in NodeSets)
        {
            var nodeSet = (INodeSet)prop.GetValue(this);
            nodeSet.Reset();
        }
    }
}