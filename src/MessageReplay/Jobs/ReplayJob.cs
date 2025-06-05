using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Data;
using Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;
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
    private static readonly Dictionary<string, ReplayJobState> jobStates = new Dictionary<string, ReplayJobState>();

    [JobDisplayName("Replaying folder - {0}")]
    public Task Run(string prefix, PerformContext context, CancellationToken cancellationToken)
    {
        var blobs = blobService
            .GetResourcesAsync(prefix, cancellationToken)
            .ToBlockingEnumerable()
            .OrderBy(x => x.CreatedOn)
            .ToList();

        var files = blobs.Select(x => x.Name).ToList();
        if (jobStates.TryGetValue(context.BackgroundJob.Id, out var jobState))
        {
            //skip until the filename is found, and this skip the blob as its already been processed
            files = files.SkipWhile(x => x != jobState.BlobName).Skip(1).ToList();
        }

        foreach (var file in files)
        {
            jobManager.Create(
                Job.FromExpression(
                    () => ProcessBlob(file, Guid.NewGuid().ToString("N"), null!, CancellationToken.None),
                    context.BackgroundJob.Job.Queue
                ),
                new EnqueuedState(context.BackgroundJob.Job.Queue)
            );

            var newJobState = new ReplayJobState() { Id = context.BackgroundJob.Id, BlobName = file };
            if (jobState is null)
            {
                jobStates.Add(context.BackgroundJob.Id, newJobState);
            }
            else
            {
                jobStates[context.BackgroundJob.Id] = newJobState;
            }

            jobState = newJobState;
        }

        return Task.CompletedTask;
    }

    [JobDisplayName("Replaying blob - {0}")]
    public async Task ProcessBlob(string file, string traceId, PerformContext context, CancellationToken token)
    {
        traceContextAccessor.Context = new TraceContext() { TraceId = traceId };
        logger.LogInformation("TraceId = {TraceId}", traceId);
        var blobItem = await blobService.GetResource(file, token);
        foreach (var blobProcessor in blobProcessors.Where(x => x.CanProcess(context.BackgroundJob.Job.Queue)))
        {
            await blobProcessor.Process(blobItem);
        }
    }

    public static void AddJobState(ReplayJobState jobState)
    {
        jobStates.Add(jobState.Id, jobState);
    }
}
