namespace Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;

public enum ResourceType
{
    ImportPreNotification,
    ClearanceRequest,
    Decision,
    Finalisation,
    Gmr,
}

public record ReplayRequest(string SourceFolder, ResourceType ResourceType);
