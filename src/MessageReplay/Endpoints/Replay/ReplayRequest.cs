namespace Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;

public record ReplayRequest(string SourceFolder, int Concurrency = 10);
