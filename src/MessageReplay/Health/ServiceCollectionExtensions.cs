using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using MongoDB.Driver;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Health;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealth(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isIntegrationTests
    )
    {
        var healthChecksBuilder = services
            .AddHealthChecks()
            .AddAzureBlobStorage(
                sp => sp.GetRequiredService<IBlobServiceClientFactory>().CreateBlobServiceClient(),
                timeout: TimeSpan.FromSeconds(15),
                tags: [WebApplicationExtensions.Extended]
            )
            .AddMongoDb(
                provider => provider.GetRequiredService<IMongoDatabase>(),
                timeout: TimeSpan.FromSeconds(10),
                tags: [WebApplicationExtensions.Extended]
            );

        if (!isIntegrationTests)
        {
            healthChecksBuilder.AddUrl(
                new Uri(configuration.GetValue<string>("GatewayOptions:HealthUri")!),
                "Gateway Api"
            );

            healthChecksBuilder.AddUrl(
                new Uri(configuration.GetValue<string>("ImportProcessorOptions:HealthUri")!),
                "Imports Processor"
            );
        }

        return services;
    }
}
