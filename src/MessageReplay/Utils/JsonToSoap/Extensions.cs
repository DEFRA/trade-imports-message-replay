using System.Xml.Linq;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

public static class Extensions
{
    public static string ToTitleCase(this string text) => char.ToUpper(text[0]) + text[1..];

    public static string ToStringWithDeclaration(this XDocument xDocument) =>
        $"{xDocument.Declaration}{Environment.NewLine}{xDocument}";
}
