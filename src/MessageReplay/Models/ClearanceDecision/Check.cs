using System.Text.Json.Serialization;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision
{
    public class Check
    {
        [JsonPropertyName("checkCode")]
        public string CheckCode { get; set; } = null!;

        [JsonPropertyName("decisionCode")]
        public string DecisionCode { get; set; } = null!;

        [JsonPropertyName("decisionValidUntil")]
        public long? DecisionValidUntil { get; set; } = null!;

        [JsonPropertyName("decisionReasons")]
        public List<string> DecisionReasons { get; set; } = null!;
    }
}
