using Serilog.Events;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Logging;

public record SetLogLevelRequest(LogEventLevel Level);
