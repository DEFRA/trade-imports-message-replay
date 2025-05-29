using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Hangfire;
using Hangfire.InMemory;
using Hangfire.Server;
using NSubstitute;

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
        blobService
            .GetResource("Test blob 1", CancellationToken.None)
            .Returns(new BlobItem() { Name = "Test blob 1", Content = BinaryData.FromString("Test blob 1") });

        var blobProcessor = Substitute.For<IBlobProcessor>();

        blobProcessor.CanProcess(Arg.Any<BlobItem>()).Returns(true);

        var sut = new ReplayJob(blobService, [blobProcessor], backgroundJobClient);
        var storage = new InMemoryStorage();
        await sut.Run(
            1,
            "Test",
            new PerformContext(
                storage,
                storage.GetConnection(),
                new BackgroundJob("test", null, DateTime.Now),
                new JobCancellationToken(false)
            )
        );

        backgroundJobClient.ReceivedWithAnyArgs(1).Create(default, default);
    }
}
