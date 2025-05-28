using System.Xml.Linq;
using Defra.TradeImportsMessageReplay.MessageReplay.Models.ClearanceDecision;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

public static class ClearanceDecisionToSoapConverter
{
    public static string Convert(ClearanceDecision clearanceDecision, string mrn)
    {
        var soapContent = new List<XElement>
        {
            new XElement(
                "ServiceHeader",
                new XElement("SourceSystem", "ALVS"),
                new XElement("DestinationSystem", "CDS"),
                new XElement("CorrelationId", clearanceDecision.ServiceHeader.CorrelationId),
                new XElement("ServiceCallTimestamp", clearanceDecision.ServiceHeader.ServiceCallTimestamp)
            ),
            new XElement(
                "Header",
                new XElement("EntryReference", mrn),
                new XElement("EntryVersionNumber", clearanceDecision.Header.EntryVersionNumber),
                new XElement("DecisionNumber", clearanceDecision.Header.DecisionNumber)
            ),
        };

        soapContent.AddRange(
            clearanceDecision.Items.Select(item => new XElement(
                "Item",
                new XElement("ItemNumber", item.ItemNumber),
                item.Checks.Select(GetCheckElement)
            ))
        );

        var soapBody = new XElement("DecisionNotification", soapContent);

        var soapMessage = SoapUtils.AddSoapEnvelope(soapBody, SoapType.AlvsToCds);

        var soapDocument = new XDocument(JsonToXmlConverter.XmlDeclaration, soapMessage);

        return soapDocument.ToStringWithDeclaration();
    }

    private static XElement GetCheckElement(Check check)
    {
        var checkElement = new XElement(
            "Check",
            new XElement("CheckCode", check.CheckCode),
            new XElement("DecisionCode", check.DecisionCode)
        );

        if (check.DecisionValidUntil.HasValue)
        {
            checkElement.Add(new XElement("DecisionValidUntil", check.DecisionValidUntil));
        }

        if (check.DecisionReasons is null)
            return checkElement;

        foreach (var checkDecisionReason in check.DecisionReasons)
        {
            if (!string.IsNullOrEmpty(checkDecisionReason))
                checkElement.Add(new XElement("DecisionReason", checkDecisionReason));
        }

        return checkElement;
    }
}
