using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;
using Hangfire;
using Hangfire.Common;
using Hangfire.InMemory;
using Hangfire.Server;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client.Extensions.Msal;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Jobs;

public class ReplayJobTests
{
    [Fact]
    public async Task When_job_run_blobs_should_be_processed()
    {
        var blobs = new List<string> { "Test blob 1" };
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
    public async Task When_replay_blob_run_blob_should_be_processed()
    {
        var blobService = Substitute.For<IBlobService>();
        var backgroundJobClient = Substitute.For<IBackgroundJobClient>();

        blobService
            .GetResource("Test blob 1", CancellationToken.None)
            .Returns(new BlobItem() { Name = "Test blob 1", Content = BinaryData.FromString("Test blob 1") });

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
            "test",
            Guid.NewGuid().ToString("N"),
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

        await blobProcessor.Received(1).Process(Arg.Any<BlobItem>());
    }
}
