using Refit;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Services;

[Headers("Content-Type: application/json")]
public interface IImportProcessorApi : IHealthApi
{
    [Post("/replay/import-pre-notifications")]
    Task SendImportPreNotification([Body] string data);

    [Post("/replay/clearance-requests")]
    Task SendClearanceRequest([Body] string data);

    [Post("/replay/finalisations")]
    Task SendFinalisation([Body] string data);
}
