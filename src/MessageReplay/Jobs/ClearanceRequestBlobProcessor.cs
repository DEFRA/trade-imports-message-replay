using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class ClearanceRequestBlobProcessor(IGatewayApi gatewayApi, ILogger<ClearanceRequestBlobProcessor> logger)
    : BlobProcessor("ALVS", logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        string soap = JsonToSoapConverter.Convert(item.Content.ToString(), "ALVSClearanceRequest", SoapType.Cds);
        await gatewayApi.SendClearanceRequest(soap);
    }
}
