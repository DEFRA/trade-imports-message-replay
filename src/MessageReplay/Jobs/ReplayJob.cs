using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs;

public class ReplayJob(
    IBlobService blobService,
    IEnumerable<IBlobProcessor> blobProcessors,
    IBackgroundJobClient jobManager,
    ITraceContextAccessor traceContextAccessor,
    ILogger<ReplayJob> logger
)
{
    [JobDisplayName("Replaying folder - {0}")]
    public async Task Run(string prefix, PerformContext context, CancellationToken cancellationToken)
    {
        var files = blobService.GetResourcesAsync(prefix, cancellationToken);

        await Parallel.ForEachAsync(
            files,
            new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = 8 },
            async (file, token) =>
            {
                await Task.Run(() => ProcessBlob(file, Guid.NewGuid().ToString("N"), context, token), token);
            }
        );
    }

    [JobDisplayName("Replaying blob - {0}")]
    public async Task ProcessBlob(string file, string traceId, PerformContext context, CancellationToken token)
    {
        try
        {
            traceContextAccessor.Context = new TraceContext() { TraceId = traceId };
            logger.LogInformation("TraceId = {TraceId}", traceId);
            var blobItem = await blobService.GetResource(file, token);
            foreach (var blobProcessor in blobProcessors.Where(x => x.CanProcess(context.BackgroundJob.Job.Queue)))
            {
                await blobProcessor.Process(blobItem);
            }
        }
        catch (Exception)
        {
            // swallow exception but add a specific continuation job, so it can be tracked and retried
            var state = new AwaitingState(
                context.BackgroundJob.Id,
                new EnqueuedState(),
                JobContinuationOptions.OnlyOnSucceededState
            );

            jobManager.Create(
                Job.FromExpression(
                    () => ProcessBlob(file, traceId, null!, CancellationToken.None),
                    context.BackgroundJob.Job.Queue
                ),
                state
            );
        }
    }
}
