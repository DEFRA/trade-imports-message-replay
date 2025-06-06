using Serilog.Core;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;

/// <summary>
/// Access to dynamically created log level switches and log filter filters.
/// </summary>
public interface ILogSwitchesAccessor
{
    /// <summary>
    /// Log level switches created either from the <c>Serilog:LevelSwitches</c> section (declared switches) or the <c>Serilog:MinimumLevel:Override</c> section (minimum level override switches).
    /// </summary>
    IDictionary<string, LoggingLevelSwitch> LogLevelSwitches { get; }
}
