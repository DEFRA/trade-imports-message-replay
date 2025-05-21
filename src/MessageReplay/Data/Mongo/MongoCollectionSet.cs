using System.Collections;
using System.Linq.Expressions;
using Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Data.Mongo;

public class MongoCollectionSet<T>(MongoDbContext dbContext, string collectionName = null!) : IMongoCollectionSet<T>
    where T : class, IDataEntity
{
    private readonly IMongoCollection<T> _collection = string.IsNullOrEmpty(collectionName)
        ? dbContext.Database.GetCollection<T>(typeof(T).Name)
        : dbContext.Database.GetCollection<T>(collectionName);

    private readonly List<T> _entitiesToInsert = [];
    private readonly List<(T Item, string Etag)> _entitiesToUpdate = [];

    private IQueryable<T> EntityQueryable => _collection.AsQueryable();

    public IEnumerator<T> GetEnumerator()
    {
        return EntityQueryable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return EntityQueryable.GetEnumerator();
    }

    public Type ElementType => EntityQueryable.ElementType;
    public Expression Expression => EntityQueryable.Expression;
    public IQueryProvider Provider => EntityQueryable.Provider;

    public int PendingChanges => _entitiesToInsert.Count + _entitiesToUpdate.Count;

    public async Task<T?> Find(string id, CancellationToken cancellationToken = default)
    {
        return await EntityQueryable.SingleOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<T?> Find(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default)
    {
        return await EntityQueryable.FirstOrDefaultAsync(query, cancellationToken: cancellationToken);
    }

    public async Task PersistAsync(CancellationToken cancellationToken)
    {
        await InsertDocuments(cancellationToken);

        await UpdateDocuments(cancellationToken);
    }

    private async Task UpdateDocuments(CancellationToken cancellationToken)
    {
        var builder = Builders<T>.Filter;

        if (_entitiesToUpdate.Count != 0)
        {
            foreach (var item in _entitiesToUpdate)
            {
                var filter = builder.Eq(x => x.Id, item.Item.Id) & builder.Eq(x => x.ETag, item.Etag);

                item.Item.ETag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
                item.Item.Updated = DateTime.UtcNow;
                item.Item.OnSave();

                var session = dbContext.ActiveTransaction?.Session;
                var updateResult = session is not null
                    ? await _collection.ReplaceOneAsync(
                        session,
                        filter,
                        item.Item,
                        cancellationToken: cancellationToken
                    )
                    : await _collection.ReplaceOneAsync(filter, item.Item, cancellationToken: cancellationToken);

                if (updateResult.ModifiedCount == 0)
                {
                    throw new ConcurrencyException(item.Item.Id!, item.Etag);
                }
            }

            _entitiesToUpdate.Clear();
        }
    }

    private async Task InsertDocuments(CancellationToken cancellationToken)
    {
        if (_entitiesToInsert.Count != 0)
        {
            foreach (var item in _entitiesToInsert)
            {
                item.ETag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
                item.Created = item.Updated = DateTime.UtcNow;
                item.OnSave();

                var session = dbContext.ActiveTransaction?.Session;
                if (session is not null)
                {
                    await _collection.InsertOneAsync(session, item, cancellationToken: cancellationToken);
                }
                else
                {
                    await _collection.InsertOneAsync(item, cancellationToken: cancellationToken);
                }
            }

            _entitiesToInsert.Clear();
        }
    }

    public Task Insert(T item, CancellationToken cancellationToken = default)
    {
        _entitiesToInsert.Add(item);
        return Task.CompletedTask;
    }

    public async Task Update(T item, CancellationToken cancellationToken = default)
    {
        await Update(item, item.ETag, cancellationToken);
    }

    public async Task Update(List<T> items, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            await Update(item, cancellationToken);
        }
    }

    public Task Update(T item, string etag, CancellationToken cancellationToken = default)
    {
        if (_entitiesToInsert.Exists(x => x.Id == item.Id))
        {
            return Task.CompletedTask;
        }

        ArgumentNullException.ThrowIfNull(etag);
        _entitiesToUpdate.RemoveAll(x => x.Item.Id == item.Id);
        _entitiesToUpdate.Add(new ValueTuple<T, string>(item, etag));
        return Task.CompletedTask;
    }
}
