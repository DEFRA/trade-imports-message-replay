using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

[ExcludeFromCodeCoverage(Justification = "Will be covered by integration tests")]
public class BlobService(IBlobServiceClientFactory blobServiceClientFactory, IOptions<BlobServiceOptions> options)
    : IBlobService
{
    private BlobContainerClient _blobContainerClient = null!;

    protected BlobContainerClient CreateBlobClient()
    {
        if (_blobContainerClient is null)
        {
            var blobServiceClient = blobServiceClientFactory.CreateBlobServiceClient();

            var containerClient = blobServiceClient.GetBlobContainerClient(options.Value.DmpBlobContainer);
            _blobContainerClient = containerClient;
        }

        return _blobContainerClient;
    }

    public async IAsyncEnumerable<BlobMetadata> GetResourcesAsync(
        string prefix,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var containerClient = CreateBlobClient();

        var itemCount = 0;

        var files = containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken);

        await foreach (var item in files)
        {
            if (item.Properties.ContentLength is not 0 && item.Name.EndsWith(".json"))
            {
                yield return new BlobMetadata(item.Name, item.Properties.CreatedOn.GetValueOrDefault());
                itemCount++;
            }
        }
    }

    public async Task<BlobItem> GetResource(string path, CancellationToken cancellationToken)
    {
        var client = CreateBlobClient();
        var blobClient = client.GetBlobClient(path);

        var content = await blobClient.DownloadContentAsync(cancellationToken);
        return new BlobItem { Name = path, Content = content.Value.Content };
    }
}
