using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using BreadTh.PersistenceAccessors.Postgres;

namespace BreadTh.PersistenceAccessors
{
    public static class AspNetConfigurationHelper
    {
        static public void AddBreadThRedisService(this IServiceCollection serviceCollection, string redisConnectionString = "localhost") =>
            serviceCollection.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(redisConnectionString));

        static public void AddBreadThPostgresService<T>(this IServiceCollection serviceCollection, Func<DbContextOptions<PostgresDbContextBase>, T> factory)
            where T : PostgresDbContextBase =>
                serviceCollection.AddSingleton<Factory<T>>(() => factory(new DbContextOptions<PostgresDbContextBase>()));

        static public void ConfigureBreadThPostgresService<T>(this IServiceProvider serviceProvider)
            where T : PostgresDbContextBase
        {
            T context = serviceProvider.GetRequiredService<Factory<T>>()();
            context.Database.EnsureCreated();
        }
    }
}
