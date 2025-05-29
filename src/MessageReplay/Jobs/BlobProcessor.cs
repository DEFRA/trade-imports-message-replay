using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public abstract class BlobProcessor(string filterValue, ILogger logger) : IBlobProcessor
{
    public bool CanProcess(BlobItem item)
    {
        return item.Name.Contains($"/{filterValue}/", StringComparison.CurrentCultureIgnoreCase);
    }

    public Task Process(BlobItem item)
    {
        logger.LogInformation("Processing blob item: {Blob}", item.Name);
        return ProcessBlobItem(item);
    }

    protected abstract Task ProcessBlobItem(BlobItem item);
}
