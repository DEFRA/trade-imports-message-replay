using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Jobs;

public class ClearanceRequestBlobProcessorTests
{
    private const string SimpleJson = """
        {
          "tag1": "data1",
          "tag2": "data2"
        }
        """;

    [Fact]
    public async Task When_receiving_clearance_request_Then_should_convert_to_soap_and_send_to_gateway()
    {
        var gatewayApi = Substitute.For<IGatewayApi>();

        var sut = new ClearanceRequestBlobProcessor(gatewayApi, NullLogger<ClearanceRequestBlobProcessor>.Instance);
        await sut.Process(new BlobItem() { Name = "Test", Content = BinaryData.FromString(SimpleJson) });

        await gatewayApi.Received(1).SendClearanceRequest(Arg.Any<string>());
    }

    [Theory]
    [InlineData(ResourceType.ClearanceRequest, true)]
    [InlineData(ResourceType.Decision, false)]
    public void When_receiving_clearance_request_can_process_depends_on_name(
        ResourceType resourceType,
        bool expectedResult
    )
    {
        var gatewayApi = Substitute.For<IGatewayApi>();

        var sut = new ClearanceRequestBlobProcessor(gatewayApi, NullLogger<ClearanceRequestBlobProcessor>.Instance);
        var result = sut.CanProcess(resourceType.ToString().ToLower());

        result.Should().Be(expectedResult);
    }
}
