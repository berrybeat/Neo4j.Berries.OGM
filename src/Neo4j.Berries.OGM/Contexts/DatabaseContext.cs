using System.Runtime.CompilerServices;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Contexts;

public sealed class DatabaseContext(Neo4jOptions neo4jOptions)
{
    public IDriver Driver { get; private set; } = neo4jOptions.Driver;
    /// <summary>
    /// This getter opens a session with database from the config. If the database is not set, it will open a session with the default database.
    /// </summary>
    public ISession Session { get; } = neo4jOptions.Driver.Session(opt =>
    {
        if (!string.IsNullOrEmpty(neo4jOptions.Database))
            opt.WithDatabase(neo4jOptions.Database);
    });
    /// <summary>
    /// This getter opens a session with database from the config. If the database is not set, it will open a session with the default database.
    /// </summary>
    public IAsyncSession AsyncSession { get; } = neo4jOptions.Driver.AsyncSession(opt =>
    {
        if (!string.IsNullOrEmpty(neo4jOptions.Database))
            opt.WithDatabase(neo4jOptions.Database);
    });

    public ITransaction Transaction { get; private set; }

    /// <summary>
    /// This method will open a transaction inside the acquired session and commit it if the action is successful, otherwise it will rollback the transaction.
    /// </summary>
    /// <param name="action">The action to be executed inside the transaction.</param>
    /// <param name="transactionConfigBuilder">The configuration of the transaction.</param>
    /// <remarks>Only one transaction can run per session!</remarks>
    public void BeginTransaction(Func<Task> action, Action<TransactionConfigBuilder> transactionConfigBuilder = null)
    {
        BeginTransaction(async () => { await action(); return 0; }, transactionConfigBuilder);
    }
    /// <summary>
    /// This method will open a transaction inside the acquired session and commit it if the action is successful, otherwise it will rollback the transaction.
    /// </summary>
    /// <param name="action">The action to be executed inside the transaction.</param>
    /// <param name="transactionConfigBuilder">The configuration of the transaction.</param>
    /// <remarks>Only one transaction can run per session!</remarks>
    public T BeginTransaction<T>(Func<Task<T>> action, Action<TransactionConfigBuilder> transactionConfigBuilder = null)
    {
        var transaction = Session.BeginTransaction(transactionConfigBuilder);
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

    /// <summary>
    /// This method will open a transaction inside the acquired session and passes the transaction to the caller. Committing and Rolling back should be handled by the caller.
    /// </summary>
    /// <param name="action">The action to be executed inside the transaction.</param>
    /// <param name="transactionConfigBuilder">The configuration of the transaction.</param>
    /// <remarks>Only one transaction can run per session!</remarks>
    public void BeingTransaction(Func<ITransaction, Task> action, Action<TransactionConfigBuilder> transactionConfigBuilder = null)
    {
        BeginTransaction(async (transaction) => { await action(transaction); return 0; }, transactionConfigBuilder);
    }
    
    /// <summary>
    /// This method will open a transaction inside the acquired session and passes the transaction to the caller. Committing and Rolling back should be handled by the caller.
    /// </summary>
    /// <param name="action">The action to be executed inside the transaction.</param>
    /// <param name="transactionConfigBuilder">The configuration of the transaction.</param>
    /// <remarks>Only one transaction can run per session!</remarks>
    public T BeginTransaction<T>(Func<ITransaction, Task<T>> action, Action<TransactionConfigBuilder> transactionConfigBuilder = null)
    {
        var transaction = Session.BeginTransaction(transactionConfigBuilder);
        Transaction = transaction;
        var result = action(transaction).Result;
        return result;
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