using Refit;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Services;

public interface IHealthApi
{
    [Get("/health/authorized")]
    Task HealthCheck();
}
