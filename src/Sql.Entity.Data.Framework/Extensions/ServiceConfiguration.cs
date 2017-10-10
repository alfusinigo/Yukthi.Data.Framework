using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
using Yc.Sql.Entity.Data.Core.Framework.Controller;
using Yc.Sql.Entity.Data.Core.Framework.Helper;
using Yc.Sql.Entity.Data.Core.Framework.Mapper;
using Yc.Sql.Entity.Data.Framework.Model.Attributes;

namespace Yc.Sql.Entity.Data.Framework.Extensions
{
    public static class ServiceConfiguration
    {
        /// <summary>
        /// DatabaseConfiguration: (SqlConnectionString), will be binded from appsettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSqlDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<DatabaseConfiguration>(
                options =>
                {
                    if (configuration.GetSection("DatabaseConfiguration:SqlConnectionString").Value == null)
                        throw new Exception($"DatabaseConfiguration:SqlConnectionString in settings file is not available!");

                    options.SqlConnectionString = configuration.GetSection("DatabaseConfiguration:SqlConnectionString").Value;
                });

            services.AddSingleton<IDatabase, SqlDatabase>();
        }

        /// <summary>
        /// A distributed cache (IDistributedCache) should be implemented
        /// CacheConfiguration: (EnableDatabaseChangeRefresh(false), ExpirationInSeconds(1800), EnableSlidingExpiration(true)), will be binded from appsettings, else defaults will be considered
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDataCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CacheConfiguration>(
                options =>
                {
                    options.EnableDatabaseChangeRefresh = Convert.ToBoolean(configuration.GetSection("CacheConfiguration:EnableDatabaseChangeRefresh").Value ?? "false");
                    options.ExpirationInSeconds = Convert.ToInt32(configuration.GetSection("CacheConfiguration:ExpirationInSeconds").Value ?? "1800");
                    options.EnableSlidingExpiration = Convert.ToBoolean(configuration.GetSection("CacheConfiguration:EnableSlidingExpiration").Value ?? "true");
                });

            services.AddSingleton<ICacheRepository, CacheRepository>();
        }

        public static void AddDataControllerAndMapper<[IsInterface]TIMapper, TMapper, [IsInterface]TIController, TController>(this IServiceCollection services)
            where TMapper : class, TIMapper, IDataMapper
            where TController : class, TIController, IDataController
            where TIMapper : class, IDataMapper
            where TIController : class, IDataController
        {
            services.AddScoped<TIMapper, TMapper>();
            services.AddScoped<IConcurrentProcessor, ConcurrentProcessor>();
            services.AddScoped<TIController, TController>();
        }
    }
}
