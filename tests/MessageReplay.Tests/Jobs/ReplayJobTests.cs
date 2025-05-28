using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using NSubstitute;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Jobs;

public class ReplayJobTests
{
    [Fact]
    public async Task When_job_run_blobs_should_be_processed()
    {
        var blobs = new List<string> { "Test blob 1" };
        var blobService = Substitute.For<IBlobService>();
        blobService.GetResourcesAsync("Test", CancellationToken.None).Returns(blobs.ToAsyncEnumerable());
        blobService
            .GetResource("Test blob 1", CancellationToken.None)
            .Returns(new BlobItem() { Name = "Test blob 1", Content = BinaryData.FromString("Test blob 1") });

        var blobProcessor = Substitute.For<IBlobProcessor>();

        blobProcessor.CanProcess(Arg.Any<BlobItem>()).Returns(true);

        var sut = new ReplayJob(blobService, [blobProcessor]);
        await sut.Run(new JobOptions(1, "Test"));

        await blobProcessor.Received(1).Process(Arg.Any<BlobItem>());
    }
}
