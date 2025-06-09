using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class FinalisationBlobProcessor(
    IImportProcessorApi importProcessorApi,
    ILogger<FinalisationBlobProcessor> logger
) : BlobProcessor(ResourceType.Finalisation, logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        await importProcessorApi.SendFinalisation(item.Content.ToString());
    }
}
