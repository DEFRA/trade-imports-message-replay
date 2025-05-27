using System.Diagnostics.CodeAnalysis;

namespace Defra.TradeImportsMessageReplay.Processor.Configuration;

[ExcludeFromCodeCoverage]
public class CdpOptions
{
    [ConfigurationKeyName("CDP_HTTPS_PROXY")]
    public string? CdpHttpsProxy { get; init; }
}
