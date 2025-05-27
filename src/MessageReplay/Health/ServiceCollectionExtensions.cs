using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using MongoDB.Driver;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Health;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealth(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddUrl(new Uri(configuration.GetValue<string>("GatewayOptions:HealthUri")!), "Gateway Api")
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

        return services;
    }
}
