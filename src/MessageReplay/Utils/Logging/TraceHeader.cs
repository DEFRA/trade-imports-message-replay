using System.ComponentModel.DataAnnotations;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;

public class TraceHeader
{
    [ConfigurationKeyName("TraceHeader")]
    [Required]
    public required string Name { get; set; }
}
