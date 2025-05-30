using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Defra.TradeImportsMessageReplay.MessageReplay.Authentication;
using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;
using Defra.TradeImportsMessageReplay.MessageReplay.Data.Extensions;
using Defra.TradeImportsMessageReplay.MessageReplay.Endpoints.Replay;
using Defra.TradeImportsMessageReplay.MessageReplay.Extensions;
using Defra.TradeImportsMessageReplay.MessageReplay.Health;
using Defra.TradeImportsMessageReplay.MessageReplay.Jobs.Extensions;
using Defra.TradeImportsMessageReplay.MessageReplay.Services;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Http;
using Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Refit;
using Serilog;
using ServiceCollectionExtensions = Defra.TradeImportsMessageReplay.MessageReplay.Data.Extensions.ServiceCollectionExtensions;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
ServiceCollectionExtensions.BootstrapMongo();

try
{
    var app = CreateWebApplication(args);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;

static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureWebApplication(builder, args);

    return BuildWebApplication(builder);
}

static void ConfigureWebApplication(WebApplicationBuilder builder, string[] args)
{
    var integrationTest = args.Contains("--integrationTest=true");

    builder.Configuration.AddJsonFile(
        $"appsettings.cdp.{Environment.GetEnvironmentVariable("ENVIRONMENT")?.ToLower()}.json",
        integrationTest
    );
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
    builder.Services.AddCustomTrustStore();

    builder.ConfigureLoggingAndTracing(integrationTest);

    // This adds default rate limiter, total request timeout, retries, circuit breaker and timeout per attempt
    builder.Services.ConfigureHttpClientDefaults(options => options.AddStandardResilienceHandler());
    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();
    builder.Services.AddHealth(builder.Configuration, integrationTest);
    builder.Services.AddHttpClient().AddHeaderPropagation();
    // Proxy HTTP Client
    builder.Services.AddTransient<ProxyHttpMessageHandler>();
    builder.Services.AddTransient<TraceContextDelegatingHandler>();
    builder.Services.AddHttpClient("proxy").ConfigurePrimaryHttpMessageHandler<ProxyHttpMessageHandler>();
    builder
        .Services.AddRefitClient<IGatewayApi>()
        .ConfigureHttpClient(c =>
            c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("GatewayOptions:BaseUri")!)
        )
        .ConfigurePrimaryHttpMessageHandler<ProxyHttpMessageHandler>()
        .AddHttpMessageHandler<TraceContextDelegatingHandler>();

    builder
        .Services.AddRefitClient<IImportProcessorApi>()
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ImportProcessorOptions:BaseUri")!);
            c.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                $"Basic {builder.Configuration.GetValue<string>("ImportProcessorOptions:Auth")}"
            );
        })
        .ConfigurePrimaryHttpMessageHandler<ProxyHttpMessageHandler>()
        .AddHttpMessageHandler<TraceContextDelegatingHandler>();

    builder.Services.AddDbContext(builder.Configuration);
    builder.Services.AddJobs();
    builder.Services.AddAuthenticationAuthorization();
    builder.Services.AddBlobStorage(builder.Configuration);

    builder
        .Services.AddHangfire(c =>
            c.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSerilogLogProvider()
                .UseConsole()
                .UseHangfireStorage(builder, integrationTest)
        )
        .AddHangfireServer(options =>
        {
            options.Queues =
            [
                ResourceType.ImportPreNotification.ToString().ToLower(),
                ResourceType.ClearanceRequest.ToString().ToLower(),
                ResourceType.Decision.ToString().ToLower(),
                ResourceType.Finalisation.ToString().ToLower(),
                ResourceType.Gmr.ToString().ToLower(),
            ];
        })
        .AddHangfireConsoleExtensions();
}

static WebApplication BuildWebApplication(WebApplicationBuilder builder)
{
    var app = builder.Build();

    app.UseHeaderPropagation();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealth();
    app.MapReplayEndpoints();
    app.UseStatusCodePages();
    app.UseHangfireDashboard();
    app.UseExceptionHandler(
        new ExceptionHandlerOptions
        {
            AllowStatusCode404Response = true,
            ExceptionHandler = async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var error = exceptionHandlerFeature?.Error;
                string? detail = null;

                if (error is BadHttpRequestException badHttpRequestException)
                {
                    context.Response.StatusCode = badHttpRequestException.StatusCode;
                    detail = badHttpRequestException.Message;
                }

                await context
                    .RequestServices.GetRequiredService<IProblemDetailsService>()
                    .WriteAsync(
                        new ProblemDetailsContext
                        {
                            HttpContext = context,
                            AdditionalMetadata = exceptionHandlerFeature?.Endpoint?.Metadata,
                            ProblemDetails = { Status = context.Response.StatusCode, Detail = detail },
                        }
                    );
            },
        }
    );

    return app;
}

#pragma warning disable S2094
namespace Defra.TradeImportsMessageReplay.MessageReplay
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program;
}
#pragma warning restore S2094
