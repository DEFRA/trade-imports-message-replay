namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddTransient<ReplayJob>();
        services.AddTransient<IBlobProcessor, ClearanceRequestBlobProcessor>();
        services.AddTransient<IBlobProcessor, DecisionBlobProcessor>();
        services.AddTransient<IBlobProcessor, FinalisationBlobProcessor>();
        services.AddTransient<IBlobProcessor, ImportPreNotificationBlobProcessor>();

        return services;
    }
}
