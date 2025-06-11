using Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;
using FluentAssertions.Common;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.JsonToSoap;

public class ClearanceDecisionToSoapConverterTests
{
    [Fact]
    public async Task When_receiving_clearance_decision_Then_should_convert_to_soap()
    {
        var clearanceDecision = new ClearanceDecision
        {
            Header = new Header { DecisionNumber = 1, EntryVersionNumber = 1 },
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

        var result = ClearanceDecisionToSoapConverter.Convert(clearanceDecision, "MRN123");

        await Verify(result);
    }
}
