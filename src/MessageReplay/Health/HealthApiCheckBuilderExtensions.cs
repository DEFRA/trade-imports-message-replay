using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Health;

[ExcludeFromCodeCoverage]
public static class HealthApiCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddHealthApiCheck<T>(
        this IHealthChecksBuilder builder,
        string name,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default
    )
        where T : IHealthApi
    {
        return builder.Add(
            new HealthCheckRegistration(
                name,
                sp =>
                {
                    var healthCheck = sp.GetRequiredService<T>();
                    return new HealthApiCheck(healthCheck);
                },
                failureStatus,
                tags,
                timeout
            )
        );
    }
}
