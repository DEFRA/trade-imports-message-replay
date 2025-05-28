using Defra.TradeImportsMessageReplay.MessageReplay.Tests;

namespace BtmsGateway.Test.Services.Converter.Fixtures;

public class JsonToXmlTestData : TheoryData<string, string, string>
{
    public JsonToXmlTestData()
    {
        Add("Simple empty JSON", JsonEmpty, "Root");
        Add("Simple null JSON property", JsonSimpleNullProperty, "Root");
        Add("Simple empty JSON property", JsonSimpleEmptyProperty, "Root");
        Add("Simple JSON property", JsonSimpleProperty, "Root");
        Add("Complex JSON single level", JsonComplexSingleLevel, "Root");
        Add("Complex JSON multi level", JsonComplexMultiLevel, "Root");
        Add("Complex JSON multi level with multi item arrays", JsonComplexMultiLevelWithArrays, "Root");
        Add("Complex JSON multi level with single item arrays", JsonComplexMultiLevelWithSingleItemArrays, "Root");
    }

    private const string JsonEmpty = "{}";

    private const string JsonSimpleNullProperty = """
        {
          "data": null
        }
        """;

    private const string JsonSimpleEmptyProperty = """
        {
          "data": ""
        }
        """;

    private const string JsonSimpleProperty = """
        {
          "data": "value1"
        }
        """;

    private const string JsonComplexSingleLevel = """
        {
          "tag1": "data1",
          "tag2": "data2"
        }
        """;

    private const string JsonComplexMultiLevel = """
        {
          "element1": {
            "tag1": "data1",
            "tag2": 12.3
          },
          "element2": {
            "tag3": true,
            "element3": {
              "tag4": null,
              "tag5": "",
              "tag6": 123,
              "tag7": "abc\ndef\nghi"
            }
          }
        }
        """;

    private const string JsonComplexMultiLevelWithArrays = """
        {
          "items": [
            {
              "tag1": "dataA",
              "tag2": 123
            },
            {
              "tag1": "dataB",
              "tag2": 456,
              "documents": [
                {
                  "tag3": "dataC",
                  "tag4": "777"
                },
                {
                  "tag3": "dataD",
                  "tag4": "888"
                }
              ],
              "checks": [
                {
                  "tag1": "dataD",
                  "tag2": "dataE"
                }
              ]
            }
          ]
        }
        """;

    private const string JsonComplexMultiLevelWithSingleItemArrays = """
        {
          "items": [
            {
              "tag1": "dataB",
              "tag2": 456,
              "documents": [
                {
                  "tag3": "dataC",
                  "tag4": "777"
                }
              ]
            }
          ]
        }
        """;
}
