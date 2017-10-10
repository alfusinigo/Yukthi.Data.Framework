using System;
using System.Collections.Generic;
using System.Text;

namespace Yc.Sql.Entity.Data.Core.Framework.Cache
{
    public class InternalCacheConfiguration
    {
        //Default 30 Minutes
        public int ExpirationInSeconds { get; set; } = 1800;

        //Default to true
        public bool EnableInMemorySlidingExpiration { get; set; } = true;

        //Default to false
        public bool EnableDatabaseChangeRefresh { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"ExpirationInSeconds: {ExpirationInSeconds}");
            builder.AppendLine($"EnableInMemorySlidingExpiration: {EnableInMemorySlidingExpiration}");
            builder.AppendLine($"EnableDatabaseChangeRefresh: {EnableDatabaseChangeRefresh}");
            return builder.ToString();
        }
    }
}
