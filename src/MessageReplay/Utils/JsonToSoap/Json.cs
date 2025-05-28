using System.Text.Json;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.JsonToSoap;

public static class Json
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };
}
