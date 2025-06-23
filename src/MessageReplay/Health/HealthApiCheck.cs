using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Health
{
    [ExcludeFromCodeCoverage]
    public class HealthApiCheck(IHealthApi healthApi) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"{nameof(HealthApiCheck)} execution is cancelled."
                );
            }

            try
            {
                await healthApi.HealthCheck();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
