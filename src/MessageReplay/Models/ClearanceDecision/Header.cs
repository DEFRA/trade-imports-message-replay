using System.Text.Json.Serialization;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;

public class Header
{
    [JsonPropertyName("entryReference")]
    public string EntryReference { get; set; } = null!;

    [JsonPropertyName("entryVersionNumber")]
    public int EntryVersionNumber { get; set; }

    [JsonPropertyName("decisionNumber")]
    public int DecisionNumber { get; set; }
}
