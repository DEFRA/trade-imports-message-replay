using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class ImportPreNotificationBlobProcessor(
    IImportProcessorApi api,
    ILogger<ImportPreNotificationBlobProcessor> logger
) : BlobProcessor(ResourceType.ImportPreNotification, logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        await api.SendImportPreNotification(item.Content.ToString());
    }
}
