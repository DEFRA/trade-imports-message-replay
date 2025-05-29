using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Jobs;

public class ImportPreNotificationBlobProcessorTests
{
    private const string SimpleJson = """
        {
          "tag1": "data1",
          "tag2": "data2"
        }
        """;

    [Fact]
    public async Task When_receiving_notification_request_Then_should_convert_to_soap_and_send_to_processor()
    {
        var api = Substitute.For<IImportProcessorApi>();

        var sut = new ImportPreNotificationBlobProcessor(api, NullLogger<ImportPreNotificationBlobProcessor>.Instance);
        await sut.Process(new BlobItem() { Name = "Test", Content = BinaryData.FromString(SimpleJson) });

        await api.Received(1).SendImportPreNotification(Arg.Any<string>());
    }

    [Theory]
    [InlineData(ResourceType.ImportPreNotification, true)]
    [InlineData(ResourceType.Decision, false)]
    public void When_receiving_notification_request_can_process_depends_on_name(
        ResourceType resourceType,
        bool expectedResult
    )
    {
        var api = Substitute.For<IImportProcessorApi>();

        var sut = new ImportPreNotificationBlobProcessor(api, NullLogger<ImportPreNotificationBlobProcessor>.Instance);
        var result = sut.CanProcess(resourceType.ToString().ToLower());

        result.Should().Be(expectedResult);
    }
}
