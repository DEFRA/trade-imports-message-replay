using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class DecisionBlobProcessor(IDecisionComparerApi decisionComparerApi, ILogger<DecisionBlobProcessor> logger)
    : BlobProcessor(ResourceType.Decision, logger)
{
    protected override async Task ProcessBlobItem(BlobItem item)
    {
        var decision = item.Content.ToObjectFromJson<ClearanceDecision>();
        if (decision == null)
            throw new ArgumentException(nameof(decision));
        
        var soap = ClearanceDecisionToSoapConverter.Convert(decision, decision.Header.EntryReference);
        
        await decisionComparerApi.SendAlvsDecision(decision.Header.EntryReference, soap);
    }
}
