using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class ClearanceRequestBlobProcessor(
    IImportProcessorApi importProcessorApi,
    ILogger<ClearanceRequestBlobProcessor> logger
) : BlobProcessor(ResourceType.ClearanceRequest, logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        await importProcessorApi.SendClearanceRequest(item.Content.ToString());
    }
}
