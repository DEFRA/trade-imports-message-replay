using System.Diagnostics.CodeAnalysis;
using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

[ExcludeFromCodeCoverage]
public class BlobServiceClientFactory(
    IServiceProvider serviceProvider,
    IOptions<BlobServiceOptions> defaultOptions,
    IHttpClientFactory? clientFactory = null
) : IBlobServiceClientFactory
{
    public BlobServiceClient CreateBlobServiceClient()
    {
        var bcOptions = new BlobClientOptions
        {
            Transport = BuildTransport()!,
            Retry =
            {
                MaxRetries = defaultOptions.Value.Retries,
                NetworkTimeout = TimeSpan.FromSeconds(defaultOptions.Value.Timeout),
            },
            Diagnostics = { IsLoggingContentEnabled = true, IsLoggingEnabled = true },
        };

        switch (defaultOptions.Value.CredentialType)
        {
            case nameof(ConfidentialClientApplicationTokenCredential):
                return new BlobServiceClient(
                    new Uri(defaultOptions.Value.DmpBlobUri),
                    new ConfidentialClientApplicationTokenCredential(serviceProvider, defaultOptions.Value),
                    bcOptions
                );
            case nameof(StorageSharedKeyCredential):
                return new BlobServiceClient(
                    new Uri(defaultOptions.Value.DmpBlobUri),
                    new StorageSharedKeyCredential(
                        defaultOptions.Value.AzureClientId,
                        defaultOptions.Value.AzureClientSecret
                    ),
                    bcOptions
                );
        }

        throw new NotSupportedException();
    }

    private HttpClientTransport? BuildTransport()
    {
        if (clientFactory != null)
        {
            return new HttpClientTransport(clientFactory.CreateClient("proxy"));
        }

        return null;
    }
}
