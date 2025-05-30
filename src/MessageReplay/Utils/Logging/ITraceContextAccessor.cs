namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;

public interface ITraceContextAccessor
{
    /// <summary>
    /// Gets or sets the current context.
    /// </summary>
    ITraceContext? Context { get; set; }
}
