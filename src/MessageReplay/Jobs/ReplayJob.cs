using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
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
    private static readonly Dictionary<string, ReplayJobState> s_jobStates = new();

    [JobDisplayName("Replaying folder - {0}")]
    public Task Run(string prefix, PerformContext context, CancellationToken cancellationToken)
    {
        var blobs = blobService
            .GetResourcesAsync(prefix, cancellationToken)
            .ToBlockingEnumerable(cancellationToken)
            .OrderBy(x => x.CreatedOn)
            .ToList();

        var files = blobs.Select(x => x.Name).ToList();
        if (s_jobStates.TryGetValue(context.BackgroundJob.Id, out var jobState))
        {
            // If the job is being retried, and we have state for the job ID, skip
            // until the filename is found; this will then continue where it finished previously
            files = files.SkipWhile(x => x != jobState.BlobName).Skip(1).ToList();
        }

        var fileGrouping = files.GroupBy(x => Path.GetFileName(x).Split("-")[0]).ToList();

        foreach (var file in fileGrouping)
        {
            jobManager.Create(
                Job.FromExpression(
                    () => ProcessBlob(file.ToArray(), Guid.NewGuid().ToString("N"), null!, CancellationToken.None),
                    context.BackgroundJob.Job.Queue
                ),
                new EnqueuedState(context.BackgroundJob.Job.Queue)
            );

            var newJobState = new ReplayJobState { Id = context.BackgroundJob.Id, BlobName = file.First() };
            if (jobState is null)
            {
                s_jobStates.Add(context.BackgroundJob.Id, newJobState);
            }
            else
            {
                s_jobStates[context.BackgroundJob.Id] = newJobState;
            }

            jobState = newJobState;
        }

        return Task.CompletedTask;
    }

    [JobDisplayName("Replaying blob - {0}")]
    public async Task ProcessBlob(string[] files, string traceId, PerformContext context, CancellationToken token)
    {
        traceContextAccessor.Context = new TraceContext { TraceId = traceId };

        logger.LogInformation("TraceId {TraceId}", traceId);

        foreach (var file in files)
        {
            var blobItem = await blobService.GetResource(file, token);

            foreach (var blobProcessor in blobProcessors.Where(x => x.CanProcess(context.BackgroundJob.Job.Queue)))
            {
                await blobProcessor.Process(blobItem);
            }
        }
    }

    public static void AddJobState(ReplayJobState jobState)
    {
        s_jobStates.Add(jobState.Id, jobState);
    }
}
