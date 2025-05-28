using Defra.TradeImportsMessageReplay.MessageReplay.Data.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Authentication.AWS;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .ValidateDataAnnotations();

        ////services.AddHostedService<MongoIndexService>();

        BootstrapMongo();

        services.AddScoped<IDbContext, MongoDbContext>();
        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetService<IOptions<MongoDbOptions>>();
            var settings = MongoClientSettings.FromConnectionString(options?.Value.DatabaseUri);

            settings.ClusterConfigurator = cb =>
                cb.Subscribe(
                    new DiagnosticsActivityEventSubscriber(new InstrumentationOptions { CaptureCommandText = true })
                );

            var client = new MongoClient(settings);

            return client;
        });
        services.AddSingleton(sp =>
        {
            var options = sp.GetService<IOptions<MongoDbOptions>>();
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(options?.Value.DatabaseName);
        });

        return services;
    }

    private static void BootstrapMongo()
    {
        try
        {
            MongoClientSettings.Extensions.AddAWSAuthentication();
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String),
            };

            ConventionRegistry.Register(nameof(conventionPack), conventionPack, _ => true);
        }
        catch (Exception)
        {
            // swallow as its already been registered
        }
    }
}
