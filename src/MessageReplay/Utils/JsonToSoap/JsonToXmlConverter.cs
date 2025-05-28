using System.Text.Json;
using System.Xml.Linq;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

public static class JsonToXmlConverter
{
    public static readonly XDeclaration XmlDeclaration = new("1.0", "utf-8", null);

    public static string Convert(string json, string rootName)
    {
        try
        {
            var rootElement = ConvertToElement(json, rootName);
            var xDocument = new XDocument(XmlDeclaration, rootElement);

            return xDocument.ToStringWithDeclaration();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid JSON", ex);
        }
    }

    public static XElement ConvertToElement(string json, string rootName)
    {
        var jsonObject = JsonSerializer.Deserialize<dynamic>(json, Json.SerializerOptions);
        var rootElement = new XElement(rootName);
        AddElements(rootElement, jsonObject);
        return rootElement;
    }

    private static void AddElements(XElement parentElement, dynamic jsonObject)
    {
        if (jsonObject is not JsonElement jsonElement)
            return;

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in jsonElement.EnumerateObject())
                {
                    var elementName = property.Name.ToTitleCase();
                    var arrayItemName = DomainInfo
                        .KnownArrays.SingleOrDefault(x => x.ArrayName == elementName)
                        ?.ItemName;
                    if (property.Value.ValueKind == JsonValueKind.Array && arrayItemName != null)
                    {
                        AddArrayElements(parentElement, property.Value, arrayItemName);
                    }
                    else
                    {
                        var childElement = new XElement(elementName);
                        AddElements(childElement, property.Value);
                        parentElement.Add(childElement);
                    }
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var arrayItemElement = new XElement("Item");
                    AddElements(arrayItemElement, item);
                    parentElement.Add(arrayItemElement);
                }
                break;

            case JsonValueKind.String:
                parentElement.Value = jsonElement.ToString();
                break;

            case JsonValueKind.Number:
                parentElement.Value = jsonElement.GetRawText();
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                parentElement.Value = jsonElement.GetBoolean().ToString();
                break;

            case JsonValueKind.Null:
                ////parentElement.Value = "null";
                break;

            case JsonValueKind.Undefined:
                break;
        }
    }

    private static void AddArrayElements(XElement parentElement, dynamic jsonObject, string arrayItemName = "")
    {
        if (jsonObject is not JsonElement jsonElement)
            return;

        foreach (var item in jsonElement.EnumerateArray())
        {
            var arrayItemElement = new XElement(arrayItemName);
            AddElements(arrayItemElement, item);
            parentElement.Add(arrayItemElement);
        }
    }
}
