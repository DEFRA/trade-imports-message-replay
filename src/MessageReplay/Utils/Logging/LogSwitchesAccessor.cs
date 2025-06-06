using Serilog.Core;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;

internal class LogSwitchesAccessor : ILogSwitchesAccessor
{
    public IDictionary<string, LoggingLevelSwitch> LogLevelSwitches { get; } =
        new Dictionary<string, LoggingLevelSwitch>();
}
