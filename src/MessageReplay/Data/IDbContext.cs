using Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Data;

public interface IDbContext
{
    IMongoCollectionSet<ReplayJobState> ReplayJobStates { get; }

    Task SaveChangesAsync(CancellationToken cancellation = default);
}
