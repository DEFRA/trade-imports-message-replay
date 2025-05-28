using System.Diagnostics.CodeAnalysis;
using Defra.TradeImportsMessageReplay.MessageReplay.Data;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

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
            var client = builder.Services.BuildServiceProvider().GetRequiredService<IMongoClient>();
            var mongoOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
            hangfireConfiguration.UseMongoStorage(
                client,
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
