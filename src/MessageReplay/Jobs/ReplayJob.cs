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
    IDbContext dbContext,
    IEnumerable<IBlobProcessor> blobProcessors,
    IBackgroundJobClient jobManager,
    ITraceContextAccessor traceContextAccessor,
    ILogger<ReplayJob> logger
)
{
    [JobDisplayName("Replaying folder - {0}")]
    public async Task Run(string prefix, PerformContext context, CancellationToken cancellationToken)
    {
        var blobs = blobService
            .GetResourcesAsync(prefix, cancellationToken)
            .ToBlockingEnumerable()
            .OrderBy(x => x.CreatedOn)
            .ToList();
        var jobState = await dbContext.ReplayJobStates.Find(context.BackgroundJob.Id, cancellationToken);
        var files = blobs.Select(x => x.Name).ToList();

        if (jobState is not null)
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
                await dbContext.ReplayJobStates.Insert(newJobState, cancellationToken);
            }
            else
            {
                await dbContext.ReplayJobStates.Update(newJobState, jobState.ETag, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            jobState = newJobState;
        }
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
}
