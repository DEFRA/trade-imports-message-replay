using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Defra.TradeImportsMessageReplay.Api.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Tests.Endpoints;
using Xunit.Abstractions;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Tests.Health;

public class PostTests(MessageReplayWebApplicationFactory factory, ITestOutputHelper outputHelper)
    : EndpointTestBase(factory, outputHelper)
{
    [Fact]
    public async Task Post_Replay_ReturnsAnonymous()
    {
        var client = CreateClient(addDefaultAuthorizationHeader: false);

        var response = await client.PostAsync("/replay", new StringContent(JsonSerializer.Serialize(new ReplayRequest()), new MediaTypeHeaderValue("application/json")));

        await Verify(response);
        await Verify(await response.Content.ReadAsStringAsync())
            .UseMethodName(nameof(Post_Replay_ReturnsAnonymous) + "_content");
    }

    [Fact]
    public async Task Post_Replay_ReturnsUnauthorized()
    {
        var client = CreateClient(addDefaultAuthorizationHeader: false);

        var response = await client.PostAsync("/replay", new  StringContent(JsonSerializer.Serialize(new ReplayRequest()), new MediaTypeHeaderValue("application/json")));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Replay_WhenAuthorized_ReturnsOk()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/replay", new StringContent(JsonSerializer.Serialize(new ReplayRequest()), new MediaTypeHeaderValue("application/json")));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
