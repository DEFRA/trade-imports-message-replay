using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;
using Hangfire;
using Hangfire.Common;
using Hangfire.InMemory;
using Hangfire.Server;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Jobs;

public class ReplayJobTests
{
    [Fact]
    public async Task When_job_run_blobs_should_be_processed()
    {
        var blobs = new List<BlobMetadata> { new("folder/test-file.json", DateTimeOffset.Now) };
        var blobService = Substitute.For<IBlobService>();
        var backgroundJobClient = Substitute.For<IBackgroundJobClient>();
        blobService.GetResourcesAsync("Test", CancellationToken.None).Returns(blobs.ToAsyncEnumerable());

        var blobProcessor = Substitute.For<IBlobProcessor>();

        blobProcessor.CanProcess(Arg.Any<string>()).Returns(true);

        var sut = new ReplayJob(
            blobService,
            [blobProcessor],
            backgroundJobClient,
            new TraceContextAccessor(),
            NullLogger<ReplayJob>.Instance
        );
        var storage = new InMemoryStorage();
        await sut.Run(
            "Test",
            new PerformContext(
                storage,
                storage.GetConnection(),
                new BackgroundJob(
                    "test",
                    new Job(typeof(ReplayJobTests).GetMethod(nameof(When_job_run_blobs_should_be_processed))),
                    DateTime.Now
                ),
                new JobCancellationToken(false)
            ),
            CancellationToken.None
        );

        backgroundJobClient.ReceivedWithAnyArgs(1).Create(default, default);
    }

    [Fact]
    public async Task When_job_run_blobs_and_has_state_only_not_processed_should_be_processed()
    {
        var blobs = new List<BlobMetadata>
        {
            new("Test/Test_blob-1.json", DateTimeOffset.Now),
            new("Test/Test_blob-2.json", DateTimeOffset.Now.AddHours(1)),
        };
        var blobService = Substitute.For<IBlobService>();
        var backgroundJobClient = Substitute.For<IBackgroundJobClient>();
        blobService.GetResourcesAsync("Test", CancellationToken.None).Returns(blobs.ToAsyncEnumerable());

        var blobProcessor = Substitute.For<IBlobProcessor>();
        ReplayJob.AddJobState(
            new ReplayJobState
            {
                Id = nameof(When_job_run_blobs_and_has_state_only_not_processed_should_be_processed),
                BlobName = blobs[0].Name,
                Created = DateTime.Now,
            }
        );

        blobProcessor.CanProcess(Arg.Any<string>()).Returns(true);

        var sut = new ReplayJob(
            blobService,
            [blobProcessor],
            backgroundJobClient,
            new TraceContextAccessor(),
            NullLogger<ReplayJob>.Instance
        );

        var storage = new InMemoryStorage();
        await sut.Run(
            "Test",
            new PerformContext(
                storage,
                storage.GetConnection(),
                new BackgroundJob(
                    nameof(When_job_run_blobs_and_has_state_only_not_processed_should_be_processed),
                    new Job(typeof(ReplayJobTests).GetMethod(nameof(When_job_run_blobs_should_be_processed))),
                    DateTime.Now
                ),
                new JobCancellationToken(false)
            ),
            CancellationToken.None
        );

        backgroundJobClient.ReceivedWithAnyArgs(1).Create(default, default);
    }

    [Fact]
    public async Task When_replay_blob_run_blob_should_be_processed()
    {
        var blobService = Substitute.For<IBlobService>();
        var backgroundJobClient = Substitute.For<IBackgroundJobClient>();

        blobService
            .GetResource("Test blob 1", CancellationToken.None)
            .Returns(new BlobItem { Name = "Test blob 1", Content = BinaryData.FromString("Test blob 1") });

        var blobProcessor = Substitute.For<IBlobProcessor>();

        blobProcessor.CanProcess(Arg.Any<string>()).Returns(true);

        var sut = new ReplayJob(
            blobService,
            [blobProcessor],
            backgroundJobClient,
            new TraceContextAccessor(),
            NullLogger<ReplayJob>.Instance
        );
        var storage = new InMemoryStorage();
        await sut.ProcessBlob(
            ["/folder/test-file.json"],
            Guid.NewGuid().ToString("N"),
            new PerformContext(
                storage,
                storage.GetConnection(),
                new BackgroundJob(
                    "/folder/test-file.json",
                    new Job(typeof(ReplayJobTests).GetMethod(nameof(When_job_run_blobs_should_be_processed))),
                    DateTime.Now
                ),
                new JobCancellationToken(false)
            ),
            CancellationToken.None
        );

        await blobProcessor.Received(1).Process(Arg.Any<BlobItem>());
    }
}
