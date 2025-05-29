using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public abstract class BlobProcessor(ResourceType resourceType, ILogger logger) : IBlobProcessor
{
    public bool CanProcess(string queue)
    {
        return queue == resourceType.ToString().ToLower();
    }

    public Task Process(BlobItem item)
    {
        logger.LogInformation("Processing blob item: {Blob}", item.Name);
        return ProcessBlobItem(item);
    }

    protected abstract Task ProcessBlobItem(BlobItem item);
}
