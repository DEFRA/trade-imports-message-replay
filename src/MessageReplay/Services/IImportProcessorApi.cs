using Refit;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Services;

[Headers("Content-Type: application/json")]
public interface IImportProcessorApi
{
    [Post("/replay/import-pre-notifications")]
    Task SendImportPreNotification([Body] string data);
}
