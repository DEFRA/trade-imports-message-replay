using System.Collections.Frozen;
using System.Net.Sockets;
using Serilog;
using Serilog.Debugging;
using Serilog.Settings.Configuration;
using ILogger = Serilog.ILogger;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;

public interface ITraceContext
{
    string? TraceId { get; }
}
