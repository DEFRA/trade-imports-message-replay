using System.Threading.Channels;
using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public record JobOptions(int MaxConcurrency, string Prefix);

public class ReplayJob(IBlobService blobService, IEnumerable<IBlobProcessor> blobProcessors)
{
    private Channel<BlobItem> _channel = null!;

    public async Task Run(JobOptions options)
    {
        _channel = Channel.CreateBounded<BlobItem>(options.MaxConcurrency);
        var consumerTask = StartConsumer();

        var producerTask = StartProducer(options.Prefix);

        await Task.WhenAll(producerTask, consumerTask);
    }

    private Task StartProducer(string prefix)
    {
        return Task.Run(async () =>
        {
            var files = blobService.GetResourcesAsync(prefix, CancellationToken.None);

            await Parallel.ForEachAsync(
                files,
                async (file, token) =>
                {
                    var blobItem = await blobService.GetResource(file, token);
                    await _channel.Writer.WriteAsync(blobItem, token);
                }
            );

            _channel.Writer.Complete();
        });
    }

    private Task StartConsumer()
    {
        return Task.Run(async () =>
        {
            await foreach (var item in _channel.Reader.ReadAllAsync())
            {
                foreach (var blobProcessor in blobProcessors.Where(x => x.CanProcess(item)))
                {
                    await blobProcessor.Process(item);
                }
            }
        });
    }
}
