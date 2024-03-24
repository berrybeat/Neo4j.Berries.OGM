using Neo4j.Driver;

namespace berrybeat.Neo4j.OGM.Contexts;

public sealed class DatabaseContext(Neo4jOptions neo4jOptions)
{
    public IDriver Driver { get; private set; } = neo4jOptions.Driver;
    private ISession Session { get; } = neo4jOptions.Driver.Session(opt =>
    {
        if (!string.IsNullOrEmpty(neo4jOptions.Database))
            opt.WithDatabase(neo4jOptions.Database);
    });
    private IAsyncSession AsyncSession { get; } = neo4jOptions.Driver.AsyncSession(opt =>
    {
        if (!string.IsNullOrEmpty(neo4jOptions.Database))
            opt.WithDatabase(neo4jOptions.Database);
    });

    public ITransaction Transaction { get; private set; }
    public void BeginTransaction(Func<Task> action)
    {
        BeginTransaction(async () => { await action(); return 0; });
    }
    public T BeginTransaction<T>(Func<Task<T>> action)
    {
        var transaction = Session.BeginTransaction();
        Transaction = transaction;
        try
        {
            var result = action().Result;
            transaction.Commit();
            Transaction = null;
            return result;
        }
        catch
        {
            transaction.Rollback();
            Transaction = null;
            throw;
        }
    }

    internal IEnumerable<IRecord> Run(string cypher, object parameters)
    {
        if (Transaction is not null)
        {
            return [.. Transaction.Run(cypher, parameters)];
        }
        else
        {
            return Session.Run(cypher, parameters).ToList();
        }
    }

    internal IEnumerable<T> Run<T>(string cypher, object parameters, Func<IRecord, T> map)
    {
        if (Transaction is not null)
        {
            return Transaction
                .Run(cypher, parameters)
                .ToList()
                .Select(map);
        }
        else
        {
            return Session
                .Run(cypher, parameters)
                .Select(map)
                .ToList();
        }
    }

    internal async Task<IEnumerable<IRecord>> RunAsync(string cypher, object parameters, CancellationToken cancellationToken = default)
    {
        if (Transaction is not null)
        {
            return [.. Transaction.Run(cypher, parameters)];
        }
        else
        {
            var result = await AsyncSession.RunAsync(cypher, parameters);
            return await result.ToListAsync(cancellationToken);
        }
    }
    internal async Task<IEnumerable<T>> RunAsync<T>(string cypher, object parameters, Func<IRecord, T> map, CancellationToken cancellationToken = default)
    {
        if (Transaction is not null)
        {
            return Transaction
                    .Run(cypher, parameters)
                    .ToList()
                    .Select(record => map(record));
        }
        else
        {
            var result = await AsyncSession
                .RunAsync(cypher, parameters);
            return (await result.ToListAsync(cancellationToken: cancellationToken))
                    .Select(record => map(record));
        }
    }
}