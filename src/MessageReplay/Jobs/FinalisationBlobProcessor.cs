using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class FinalisationBlobProcessor(IGatewayApi gatewayApi, ILogger<FinalisationBlobProcessor> logger)
    : BlobProcessor("FINALISATION", logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        string soap = JsonToSoapConverter.Convert(
            item.Content.ToString(),
            "FinalisationNotificationRequest",
            SoapType.Cds
        );
        await gatewayApi.SendFinalisation(soap);
    }
}
