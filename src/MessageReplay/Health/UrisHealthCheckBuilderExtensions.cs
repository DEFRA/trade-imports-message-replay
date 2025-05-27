using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Health;

[ExcludeFromCodeCoverage]
public static class UrisHealthCheckBuilderExtensions
{
    private static readonly Action<IServiceProvider, HttpClient> _emptyHttpClientCallback = (_, _) => { };

    public static IHealthChecksBuilder AddUrl(
        this IHealthChecksBuilder builder,
        Uri uri,
        string name,
        Action<IServiceProvider, HttpClient>? configureClient = null,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var registrationName = name;
        builder.Services.AddHttpClient(registrationName)
            .ConfigureHttpClient(configureClient ?? _emptyHttpClientCallback);

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new UriHealthCheck(uri, () => httpClientFactory.CreateClient(name));
            },
            failureStatus,
            tags,
            timeout));
    }

}