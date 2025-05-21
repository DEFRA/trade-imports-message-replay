namespace Defra.TradeImportsMessageReplay.MessageReplay.Data;

public interface IDbContext
{
    Task SaveChangesAsync(CancellationToken cancellation = default);
}
