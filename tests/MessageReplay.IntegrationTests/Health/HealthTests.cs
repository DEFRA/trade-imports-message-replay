using System.Net;
using Defra.TradeImportsMessageReplay.MessageReplay.IntegrationTests.Clients;
using FluentAssertions;
using WireMock.Client;
using WireMock.Client.Extensions;

namespace Defra.TradeImportsMessageReplay.MessageReplay.IntegrationTests.Health;

[Collection("UsesWireMockClient")]
public class HealthTests(WireMockClient wireMockClient) : IntegrationTestBase
{
    private readonly IWireMockAdminApi _wireMockAdminApi = wireMockClient.WireMockAdminApi;

    [Fact]
    public async Task HealthAll_ShouldBeOk()
    {
        var getMappingBuilder = _wireMockAdminApi.GetMappingBuilder();
        getMappingBuilder.Given(m =>
            m.WithRequest(req => req.UsingGet().WithPath("/health/authorized"))
                .WithResponse(rsp =>
                {
                    rsp.WithStatusCode(HttpStatusCode.OK);
                })
        );
        var getMappingBuilderResult = await getMappingBuilder.BuildAndPostAsync();
        Assert.Null(getMappingBuilderResult.Error);
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };

        var response = await client.GetAsync("/health/all");

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }
}
