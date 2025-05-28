using System.Text.Json.Serialization;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;

public class ServiceHeader
{
    [JsonPropertyName("sourceSystem")]
    public string SourceSystem { get; set; } = null!;

    [JsonPropertyName("destinationSystem")]
    public string DestinationSystem { get; set; } = null!;

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = null!;

    [JsonPropertyName("serviceCallTimestamp")]
    public long ServiceCallTimestamp { get; set; }
}
