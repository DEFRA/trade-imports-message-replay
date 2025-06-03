using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;
using MongoDB.Driver;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Data.Mongo;

[ExcludeFromCodeCoverage]
public class MongoDbContext : IDbContext
{
    public MongoDbContext(IMongoDatabase database)
    {
        Database = database;
        ReplayJobStates = new MongoCollectionSet<ReplayJobState>(this);
    }

    internal IMongoDatabase Database { get; }
    internal MongoDbTransaction? ActiveTransaction { get; private set; }

    public async Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        var session = await Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        ActiveTransaction = new MongoDbTransaction(session);
        return ActiveTransaction;
    }

    public IMongoCollectionSet<ReplayJobState> ReplayJobStates { get; }

    public async Task SaveChangesAsync(CancellationToken cancellation = default)
    {
        if (GetChangedRecordsCount() == 0)
        {
            return;
        }

        if (GetChangedRecordsCount() == 1)
        {
            await InternalSaveChangesAsync(cancellation);
            return;
        }

        using var transaction = await StartTransaction(cancellation);
        try
        {
            await InternalSaveChangesAsync(cancellation);
            await transaction.CommitTransaction(cancellation);
        }
        catch (Exception)
        {
            await transaction.RollbackTransaction(cancellation);
            throw;
        }
    }

    private int GetChangedRecordsCount()
    {
        // This logic needs to be reviewed as it's easy to forget to include any new collection sets
        return ReplayJobStates.PendingChanges;
    }

    private Task InternalSaveChangesAsync(CancellationToken cancellation = default)
    {
        // This logic needs to be reviewed as it's easy to forget to include any new collection sets
        return ReplayJobStates.PersistAsync(cancellation);
    }
}
