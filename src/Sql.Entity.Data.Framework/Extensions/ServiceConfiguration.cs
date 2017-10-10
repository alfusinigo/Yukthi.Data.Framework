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

        public static void AddInternalCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<InternalCacheConfiguration>(
                options =>
                {
                    options.ExpirationInSeconds = Convert.ToInt32(configuration.GetSection("InternalCacheConfiguration:ExpirationInSeconds").Value ?? "1800");
                    options.EnableDatabaseChangeRefresh = Convert.ToBoolean(configuration.GetSection("InternalCacheConfiguration:EnableDatabaseChangeRefresh").Value ?? "false");
                    options.EnableInMemorySlidingExpiration = Convert.ToBoolean(configuration.GetSection("InternalCacheConfiguration:EnableInMemorySlidingExpiration").Value ?? "true");
                });

            services.Configure<InternalCacheConfiguration>(options => configuration.GetSection("InternalCacheConfiguration"));

            services.AddSingleton<ICacheRepository, InternalCacheRepository>();
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
