namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

public interface IBlobService
{
    public IAsyncEnumerable<string> GetResourcesAsync(string prefix, CancellationToken cancellationToken);
    public Task<BlobItem> GetResource(string path, CancellationToken cancellationToken);
}
