using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Http;
using Defra.TradeImportsMessageReplay.Processor.Configuration;
using Microsoft.Extensions.Options;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        // The azure client has it's own way of proxying :|
        services
            .AddHttpClient("Msal")
            .ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler)
            .ConfigureHttpClient(httpClient =>
            {
                // Default MSAL settings:
                // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/4.61.3/src/client/Microsoft.Identity.Client/Http/HttpClientConfig.cs#L18-L20
                httpClient.MaxResponseContentBufferSize = 1024 * 1024;
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

        services.AddSingleton<MsalHttpClientFactoryAdapter>();

        services
            .AddOptions<BlobServiceOptions>()
            .Bind(configuration.GetSection(BlobServiceOptions.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();

        return services;
    }

    public static HttpClientHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<CdpOptions>>();
        var proxy = sp.GetRequiredService<IWebProxy>();
        return new HttpClientHandler { Proxy = proxy, UseProxy = options.Value.CdpHttpsProxy != null };
    }
}
