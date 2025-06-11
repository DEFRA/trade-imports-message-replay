using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Jobs;

public class DecisionBlobProcessorTests
{
    readonly ClearanceDecision clearanceDecision = new ClearanceDecision
    {
        Header = new Header
        {
            DecisionNumber = 1,
            EntryVersionNumber = 1,
            EntryReference = "testmrn",
        },
        ServiceHeader = new ServiceHeader
        {
            CorrelationId = "external-correlation-id",
            ServiceCallTimestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .ToDateTimeOffset()
                .ToUnixTimeMilliseconds(),
        },
        Items =
        [
            new Item
            {
                ItemNumber = 1,
                Checks =
                [
                    new Check
                    {
                        CheckCode = "H218",
                        DecisionCode = "C02",
                        DecisionValidUntil = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .ToDateTimeOffset()
                            .ToUnixTimeMilliseconds(),
                        DecisionReasons = ["Some decision reason"],
                    },
                ],
            },
            new Item
            {
                ItemNumber = 2,
                Checks =
                [
                    new Check
                    {
                        CheckCode = "H218",
                        DecisionCode = "C02",
                        DecisionValidUntil = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .ToDateTimeOffset()
                            .ToUnixTimeMilliseconds(),
                        DecisionReasons = ["Some decision reason 1", "Some decision reason 2"],
                    },
                ],
            },
            new Item
            {
                ItemNumber = 3,
                Checks =
                [
                    new Check
                    {
                        CheckCode = "H218",
                        DecisionCode = "C02",
                        DecisionValidUntil = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .ToDateTimeOffset()
                            .ToUnixTimeMilliseconds(),
                    },
                ],
            },
            new Item
            {
                ItemNumber = 4,
                Checks =
                [
                    new Check
                    {
                        CheckCode = "H218",
                        DecisionCode = "C02",
                        DecisionValidUntil = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .ToDateTimeOffset()
                            .ToUnixTimeMilliseconds(),
                        DecisionReasons = [""],
                    },
                ],
            },
        ],
    };

    [Fact]
    public async Task When_receiving_decision_request_Then_should_convert_to_soap_and_send_to_gateway()
    {
        var gatewayApi = Substitute.For<IDecisionComparerApi>();

        var sut = new DecisionBlobProcessor(gatewayApi, NullLogger<DecisionBlobProcessor>.Instance);
        await sut.Process(new BlobItem { Name = "Test", Content = BinaryData.FromObjectAsJson(clearanceDecision) });

        await gatewayApi.Received(1).SendAlvsDecision(clearanceDecision.Header.EntryReference, Arg.Any<string>());
    }

    [Theory]
    [InlineData(ResourceType.Decision, true)]
    [InlineData(ResourceType.Gmr, false)]
    public void When_receiving_decision_request_can_process_depends_on_name(
        ResourceType resourceType,
        bool expectedResult
    )
    {
        var gatewayApi = Substitute.For<IDecisionComparerApi>();

        var sut = new DecisionBlobProcessor(gatewayApi, NullLogger<DecisionBlobProcessor>.Instance);
        var result = sut.CanProcess(resourceType.ToString().ToLower());

        result.Should().Be(expectedResult);
    }
}
