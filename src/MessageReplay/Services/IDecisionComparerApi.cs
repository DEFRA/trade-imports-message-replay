using Refit;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Services
{
    public interface IDecisionComparerApi : IHealthApi
    {
        [Put("/alvs-decisions/{mrn}")]
        Task SendAlvsDecision(string mrn, [Body] string xml);
    }
}
