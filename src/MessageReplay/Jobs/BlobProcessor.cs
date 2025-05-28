using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public abstract class BlobProcessor(string filterValue, ILogger logger) : IBlobProcessor
{
    public bool CanProcess(BlobItem item)
    {
        return item.Name.Contains($"/{filterValue}/", StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task Process(BlobItem item)
    {
        try
        {
            await ProcessBlobItem(item);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process blob item: {Blob}", item.Name);
        }
    }

    protected abstract Task ProcessBlobItem(BlobItem item);
}
