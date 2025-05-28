using Defra.TradeImportsMessageReplay.MessageReplay.Authentication;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;

public static class EndpointRouteBuilderExtensions
{
    public static void MapReplayEndpoints(this IEndpointRouteBuilder app)
    {
        const string groupName = "Replay";

        app.MapPost("replay", Post)
            .WithName("Replay")
            .WithTags(groupName)
            .WithSummary("Post Replay")
            .WithDescription("Replays")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(PolicyNames.Write);
    }

    [HttpPut]
    private static IResult Post([FromBody] ReplayRequest data)
    {
        var jobId = BackgroundJob.Enqueue(() => Console.WriteLine($"Fire-and-forget! - {data.Temp}"));

        return Results.Ok(new ReplayResponse(jobId));
    }
}
