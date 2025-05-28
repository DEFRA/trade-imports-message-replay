using System.Text.Json.Serialization;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;

public class Item
{
    [JsonPropertyName("itemNumber")]
    public int ItemNumber { get; set; }

    [JsonPropertyName("documents")]
    public List<object> Documents { get; set; } = null!;

    [JsonPropertyName("checks")]
    public List<Check> Checks { get; set; } = null!;
}
