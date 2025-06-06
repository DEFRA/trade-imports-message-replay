using Defra.TradeImportsMessageReplay.MessageReplay.Authentication;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Logging;

public static class EndpointRouteBuilderExtensions
{
    public static void MapLoggingEndpoints(this IEndpointRouteBuilder app)
    {
        const string groupName = "logging";

        app.MapPost("loglevel", Post)
            .WithName("Log level")
            .WithTags(groupName)
            .WithSummary("Post Log Level")
            .WithDescription("Logging")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(PolicyNames.Write);
    }

    [HttpPut]
    private static IResult Post(
        [FromBody] SetLogLevelRequest data,
        [FromServices] ILogSwitchesAccessor switchesAccessor
    )
    {
        foreach (var switchesAccessorLogLevelSwitch in switchesAccessor.LogLevelSwitches)
        {
            switchesAccessorLogLevelSwitch.Value.MinimumLevel = data.Level;
        }

        return Results.Ok();
    }
}
