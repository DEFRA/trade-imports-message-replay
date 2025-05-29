using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class ReplayJob(
    IBlobService blobService,
    IEnumerable<IBlobProcessor> blobProcessors,
    IBackgroundJobClient jobManager
)
{
    public async Task Run(int maxConcurrency, string prefix, PerformContext context)
    {
        var files = blobService.GetResourcesAsync(prefix, CancellationToken.None);

        await Parallel.ForEachAsync(
            files,
            new ParallelOptions() { MaxDegreeOfParallelism = maxConcurrency },
            (file, token) =>
            {
                var state = new AwaitingState(
                    context.BackgroundJob.Id,
                    new EnqueuedState(),
                    JobContinuationOptions.OnlyOnSucceededState
                );
                jobManager.Create(Job.FromExpression(() => ProcessBlob(file, CancellationToken.None)), state);
                return ValueTask.CompletedTask;
            }
        );
    }

    public async Task ProcessBlob(string file, CancellationToken token)
    {
        var blobItem = await blobService.GetResource(file, token);
        foreach (var blobProcessor in blobProcessors.Where(x => x.CanProcess(blobItem)))
        {
            await blobProcessor.Process(blobItem);
        }
    }
}
