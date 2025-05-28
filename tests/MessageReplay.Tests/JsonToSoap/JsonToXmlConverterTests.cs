using BtmsGateway.Test.Services.Converter.Fixtures;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.JsonToSoap;

public class JsonToXmlConverterTests
{
    private static readonly string TestDataPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "JsonToSoap",
        "Fixtures"
    );

    [Theory]
    [ClassData(typeof(JsonToXmlTestData))]
    public async Task When_receiving_valid_json_Then_should_convert_to_xml(string because, string json, string rootName)
    {
        var xml = JsonToXmlConverter.Convert(json, rootName);
        await Verify(xml).UseMethodName($"{nameof(When_receiving_valid_json_Then_should_convert_to_xml)}_{because}");
    }

    [Fact]
    public async Task When_receiving_clearance_request_json_Then_should_convert_to_xml()
    {
        var json = await File.ReadAllTextAsync(Path.Combine(TestDataPath, "ClearanceRequest.json"));

        var xml = JsonToXmlConverter.Convert(json, "ALVSClearanceRequest");

        await Verify(xml);
    }

    [Fact]
    public void When_receiving_invalid_json_Then_should_fail()
    {
        var act = () => JsonToXmlConverter.Convert("{\"abc\"", "Root");

        act.Should().Throw<ArgumentException>();
    }
}
