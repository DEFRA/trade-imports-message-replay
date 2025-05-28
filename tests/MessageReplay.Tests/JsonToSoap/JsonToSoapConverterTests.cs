using Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.JsonToSoap;

public class JsonToSoapConverterTests
{
    private static readonly string TestDataPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "JsonToSoap",
        "Fixtures"
    );

    [Theory]
    [InlineData("ALVSClearanceRequest", SoapType.Cds, "ClearanceRequest.json")]
    [InlineData("ALVSClearanceRequest", SoapType.AlvsToIpaffs, "ClearanceRequest.json")]
    [InlineData("DecisionNotification", SoapType.AlvsToCds, "DecisionNotification.json")]
    public async Task When_receiving_clearance_request_soap_Then_should_convert_to_json(
        string rootName,
        SoapType soapType,
        string jsonFileName
    )
    {
        var json = await File.ReadAllTextAsync(Path.Combine(TestDataPath, jsonFileName));

        var xml = JsonToSoapConverter.Convert(json, rootName, soapType);

        await Verify(xml)
            .UseMethodName($"{nameof(When_receiving_clearance_request_soap_Then_should_convert_to_json)}_{soapType}");
    }
}
