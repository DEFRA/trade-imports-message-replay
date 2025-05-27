using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Health
{
    [ExcludeFromCodeCoverage]
    public class UriHealthCheck(Uri uri, Func<HttpClient> httpClientFactory) : IHealthCheck
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
                    description: $"{nameof(UriHealthCheck)} execution is cancelled."
                );
            }

            try
            {
                var httpClient = httpClientFactory();
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                requestMessage.Version = httpClient.DefaultRequestVersion;
                requestMessage.VersionPolicy = httpClient.DefaultVersionPolicy;
                using var response = await httpClient
                    .SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return new HealthCheckResult(
                        context.Registration.FailureStatus,
                        description: $"Discover endpoint #{uri} is not responding with code 2xx range, the current status is {response.StatusCode}."
                    );
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
