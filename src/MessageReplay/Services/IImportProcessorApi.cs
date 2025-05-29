using Refit;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Services;

public interface IImportProcessorApi
{
    [Post("/replay/import-pre-notifications")]
    Task SendImportPreNotification([Body] string json);
}
