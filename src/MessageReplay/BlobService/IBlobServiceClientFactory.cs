using Azure.Storage.Blobs;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

public interface IBlobServiceClientFactory
{
    BlobServiceClient CreateBlobServiceClient();
}
