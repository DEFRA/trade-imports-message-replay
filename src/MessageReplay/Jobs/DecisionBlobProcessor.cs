using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class DecisionBlobProcessor(IGatewayApi gatewayApi, ILogger<DecisionBlobProcessor> logger)
    : BlobProcessor(ResourceType.Decision, logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        var decision = item.Content.ToObjectFromJson<ClearanceDecision>();
        if (decision == null)
            throw new ArgumentException(nameof(decision));
        string soap = ClearanceDecisionToSoapConverter.Convert(decision, decision.Header.EntryReference);
        await gatewayApi.SendAlvsDecision(soap);
    }
}
