namespace Defra.TradeImportsMessageReplay.Processor.Configuration;

public class CdpOptions
{
    [ConfigurationKeyName("CDP_HTTPS_PROXY")]
    public string? CdpHttpsProxy { get; init; }
}
