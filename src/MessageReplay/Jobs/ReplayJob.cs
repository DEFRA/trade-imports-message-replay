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
    ITraceContextAccessor traceContextAccessor
)
{
    [JobDisplayName("Replaying folder - {0}")]
    public async Task Run(string prefix, PerformContext context, CancellationToken cancellationToken)
    {
        var files = blobService.GetResourcesAsync(prefix, cancellationToken);

        await foreach (var file in files)
        {
            var state = new AwaitingState(
                context.BackgroundJob.Id,
                new EnqueuedState(),
                JobContinuationOptions.OnlyOnSucceededState
            );
            jobManager.Create(
                Job.FromExpression(
                    () => ProcessBlob(file, null!, CancellationToken.None),
                    context.BackgroundJob.Job.Queue
                ),
                state
            );
        }
    }

    [JobDisplayName("Replaying blob - {0}")]
    public async Task ProcessBlob(string file, PerformContext context, CancellationToken token)
    {
        traceContextAccessor.Context = new TraceContext() { TraceId = Guid.NewGuid().ToString("N") };
        var blobItem = await blobService.GetResource(file, token);
        foreach (var blobProcessor in blobProcessors.Where(x => x.CanProcess(context.BackgroundJob.Job.Queue)))
        {
            await blobProcessor.Process(blobItem);
        }
    }
}
