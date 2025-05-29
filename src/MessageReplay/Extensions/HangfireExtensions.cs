using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.Data;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Extensions;

[ExcludeFromCodeCoverage]
public static class HangfireExtensions
{
    public static IGlobalConfiguration UseHangfireStorage(
        this IGlobalConfiguration hangfireConfiguration,
        WebApplicationBuilder builder,
        bool integrationTest
    )
    {
        if (integrationTest)
        {
            hangfireConfiguration.UseInMemoryStorage();
        }
        else
        {
            var mongoOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
            var settings = MongoClientSettings.FromConnectionString(mongoOptions?.DatabaseUri);

            settings.ClusterConfigurator = cb =>
                cb.Subscribe(
                    new DiagnosticsActivityEventSubscriber(new InstrumentationOptions { CaptureCommandText = true })
                );

            hangfireConfiguration.UseMongoStorage(
                settings,
                mongoOptions?.DatabaseName,
                new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy(),
                    },
                    Prefix = "hangfire.mongo",
                    CheckConnection = true,
                }
            );
        }

        return hangfireConfiguration;
    }
}
