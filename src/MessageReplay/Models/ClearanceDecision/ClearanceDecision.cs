using System.Text.Json.Serialization;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;

public class ClearanceDecision
{
    [JsonPropertyName("serviceHeader")]
    public ServiceHeader ServiceHeader { get; set; } = null!;

    [JsonPropertyName("header")]
    public Header Header { get; set; } = null!;

    [JsonPropertyName("items")]
    public List<Item> Items { get; set; } = null!;
}
